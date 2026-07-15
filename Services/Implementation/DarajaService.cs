using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PayGate.DTOs.Daraja;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class DarajaService: IDarajaService
{
    private readonly HttpClient _httpClient;
    private   readonly  ILogger<DarajaService> _logger;
    
    public  DarajaService(HttpClient httpClient, ILogger<DarajaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<string> GetAccessTokenAsync(string consumerKey, string consumerSecret, string baseUrl)
    {
        if (string.IsNullOrEmpty(consumerKey) || string.IsNullOrEmpty(consumerSecret))
            throw new Exception("Daraja Consumer Key or Secret is missing.");

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
        
        // Use DefaultRequestHeaders like the reference code
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var response = await _httpClient.GetAsync($"{baseUrl}/oauth/v1/generate?grant_type=client_credentials");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get access token. Status: {response.StatusCode}. Error: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        return result.GetProperty("access_token").GetString() 
            ?? throw new Exception("Failed to extract access_token from Daraja response.");
    }

    public async Task<DarajaStkPushResponse> SendStkPushAsync(
        string baseUrl, 
        string accessToken, 
        DarajaStkPushRequest request)
    {
        // Clear any previous headers and set the Bearer token
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Serialize the request
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("📡 Sending STK Push to: {Url}", $"{baseUrl}/mpesa/stkpush/v1/processrequest");
        _logger.LogInformation("📡 Request Body: {Body}", json);

        var response = await _httpClient.PostAsync($"{baseUrl}/mpesa/stkpush/v1/processrequest", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("📡 Daraja STK Push Response Status: {StatusCode}", response.StatusCode);
        _logger.LogInformation("📡 Daraja STK Push Response Body: {Body}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("❌ Daraja STK Push FAILED: {Error}", responseContent);
            throw new Exception($"Daraja API Error ({response.StatusCode}): {responseContent}");
        }

        return JsonSerializer.Deserialize<DarajaStkPushResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        }) ?? throw new Exception("Failed to parse STK push response.");
    }
}