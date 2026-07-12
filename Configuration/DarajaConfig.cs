namespace PayGate.Configuration;

public class DarajaConfig
{
    // Base URLs for the API
    public string SandboxBaseUrl { get; set; } = "https://sandbox.safaricom.co.ke";
    public string ProductionBaseUrl { get; set; } = "https://api.safaricom.co.ke";

    // Specific Endpoints
    public string OAuthUrl { get; set; } = "/oauth/v1/generate?grant_type=client_credentials";
    public string StkPushUrl { get; set; } = "/mpesa/stkpush/v1/processrequest";
    public string StkPushQueryUrl { get; set; } = "/mpesa/stkpushquery/v1/query";
}