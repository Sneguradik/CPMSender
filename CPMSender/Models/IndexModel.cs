using System.Text.Json.Serialization;

namespace CPMSender.Models;

public class IndexModel
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("value")]
    public double Value { get; set; }
}