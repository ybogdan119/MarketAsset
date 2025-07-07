using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents the payload of a WebSocket price update message,
/// containing the most recent trade information (last price).
/// </summary>
public class WsPriceData
{
    /// <summary>
    /// The most recent price point received via WebSocket.
    /// Can be null if no "last" data is included in the message.
    /// </summary>
    [JsonPropertyName("last")]
    public PricePoint? Last { get; set; }
}
