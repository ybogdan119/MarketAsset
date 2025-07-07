using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents a single historical price data point (OHLCV) for an asset.
/// </summary>
public class HistPriceData
{
    /// <summary>
    /// The timestamp of the price data.
    /// </summary>
    [JsonPropertyName("t")]
    public DateTime Time { get; set; }

    /// <summary>
    /// The opening price of the asset at the given time.
    /// </summary>
    [JsonPropertyName("o")]
    public double Open { get; set; }

    /// <summary>
    /// The highest price during the given time interval.
    /// </summary>
    [JsonPropertyName("h")]
    public double High { get; set; }

    /// <summary>
    /// The lowest price during the given time interval.
    /// </summary>
    [JsonPropertyName("l")]
    public double Low { get; set; }

    /// <summary>
    /// The closing price at the end of the time interval.
    /// </summary>
    [JsonPropertyName("c")]
    public double Close { get; set; }

    /// <summary>
    /// The trading volume during the time interval.
    /// </summary>
    [JsonPropertyName("v")]
    public long Volume { get; set; }
}
