using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents a single price point received from the WebSocket stream, including the price and its timestamp.
/// </summary>
public class PricePoint
{
    /// <summary>
    /// The last traded price of the instrument.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// The timestamp when the price was recorded.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
