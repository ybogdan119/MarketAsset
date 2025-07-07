using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents the response containing a list of historical price data points.
/// </summary>
public class HistPriceResponse
{
    /// <summary>
    /// A list of historical price data entries (OHLCV).
    /// Can be null if the response contains no data.
    /// </summary>
    [JsonPropertyName("data")]
    public List<HistPriceData>? Data { get; set; }
}
