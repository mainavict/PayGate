using System.Text.Json.Serialization;

namespace PayGate.DTOs.Daraja;

public class DarajaStkPushRequest
{
    [JsonPropertyName("BusinessShortCode")]
    public string BusinessShortCode { get; set; } = string.Empty;

    [JsonPropertyName("Password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("Timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("TransactionType")]
    public string TransactionType { get; set; } = "CustomerPayBillOnline";

    [JsonPropertyName("Amount")]
    public int Amount { get; set; }

    [JsonPropertyName("PartyA")]
    public string PartyA { get; set; } = string.Empty; // Customer Phone Number

    [JsonPropertyName("PartyB")]
    public string PartyB { get; set; } = string.Empty; // Shortcode

    [JsonPropertyName("PhoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty; // Customer Phone Number

    [JsonPropertyName("CallBackURL")]
    public string CallBackURL { get; set; } = string.Empty; // Your Webhook URL

    [JsonPropertyName("AccountReference")]
    public string AccountReference { get; set; } = string.Empty;

    [JsonPropertyName("TransactionDesc")]
    public string TransactionDesc { get; set; } = string.Empty;
}