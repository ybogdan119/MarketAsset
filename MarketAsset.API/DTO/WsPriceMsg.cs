using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents a WebSocket message received from the price stream,
/// containing information about the instrument and the latest price update.
/// </summary>
public class WsPriceMsg
{
    /// <summary>
    /// The type of WebSocket message (e.g. "l1-update", "response", "session").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the instrument this message refers to.
    /// </summary>
    [JsonPropertyName("instrumentId")]
    public string InstrumentId { get; set; } = string.Empty;

    /// <summary>
    /// The most recent price point data (optional).
    /// Only present if the message type is "l1-update".
    /// </summary>
    [JsonPropertyName("last")]
    public PricePoint? Last { get; set; }
}
