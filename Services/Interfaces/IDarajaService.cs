using PayGate.DTOs.Daraja;

namespace PayGate.Services.Interfaces;

public interface IDarajaService
{
    // 1. Get the OAuth token from Safaricom
    Task<string> GetAccessTokenAsync(string consumerKey, string consumerSecret, string baseUrl);

    // 2. Send the STK Push to the user's phone
    Task<DarajaStkPushResponse> SendStkPushAsync(
        string baseUrl,
        string accessToken,
        DarajaStkPushRequest request);
}