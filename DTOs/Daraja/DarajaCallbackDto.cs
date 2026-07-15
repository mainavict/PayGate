using System.Text.Json.Serialization;

namespace PayGate.DTOs.Daraja;

public class DarajaCallbackDto
{
    [JsonPropertyName("Body")]
    public DarajaBody? Body { get; set; }
}

public class DarajaBody
{
    [JsonPropertyName("stkCallback")]
    public StkCallback? stkCallback { get; set; }
}

public class StkCallback
{
    [JsonPropertyName("MerchantRequestID")]
    public string? MerchantRequestID { get; set; }
    
    [JsonPropertyName("CheckoutRequestID")]
    public string? CheckoutRequestID { get; set; }
    
    [JsonPropertyName("ResultCode")]
    public int ResultCode { get; set; } // 0 means Success!
    
    [JsonPropertyName("ResultDesc")]
    public string? ResultDesc { get; set; }
    
    [JsonPropertyName("CallbackMetadata")]
    public CallbackMetadata? CallbackMetadata { get; set; }
}

public class CallbackMetadata
{
    [JsonPropertyName("Item")]
    public List<CallbackItem>? Item { get; set; }
}

public class CallbackItem
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("Value")]
    public string? Value { get; set; } 
}