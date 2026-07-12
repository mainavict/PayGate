using System.Text.Json.Serialization;

namespace PayGate.DTOs.Daraja;

public class DarajaStkPushResponse
{
    [JsonPropertyName("MerchantRequestID")]
    public string? MerchantRequestID { get; set; }

    [JsonPropertyName("CheckoutRequestID")]
    public string? CheckoutRequestID { get; set; }

    [JsonPropertyName("ResponseCode")]
    public string? ResponseCode { get; set; }

    [JsonPropertyName("ResponseDescription")]
    public string? ResponseDescription { get; set; }

    [JsonPropertyName("CustomerMessage")]
    public string? CustomerMessage { get; set; }
}