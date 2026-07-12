using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PayGate.DTOs.Daraja;
using PayGate.Services.Interfaces;

namespace PayGate.Services.Implementation;

public class DarajaService : IDarajaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DarajaService> _logger;

    public DarajaService(HttpClient httpClient, ILogger<DarajaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(string consumerKey, string consumerSecret, string baseUrl)
    {
        // Combine the keys for Basic Authentication
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
        
        // Set up the request
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/oauth/v1/generate?grant_type=client_credentials");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        // Send the request
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Read the response
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        // Extract the access token
        return json.RootElement.GetProperty("access_token").GetString() 
            ?? throw new Exception("Failed to get access token from Daraja.");
    }

    public async Task<DarajaStkPushResponse> SendStkPushAsync(
        string baseUrl, 
        string accessToken, 
        DarajaStkPushRequest request)
    {
        // Set up the request with the Bearer token
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/mpesa/stkpush/v1/processrequest");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        // Add the JSON body
        httpRequest.Content = JsonContent.Create(request);

        // Send the request
        var response = await _httpClient.SendAsync(httpRequest);
        
        // Read the response
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Map it to our DTO
        return JsonSerializer.Deserialize<DarajaStkPushResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        }) ?? throw new Exception("Failed to deserialize Daraja response.");
    }
}