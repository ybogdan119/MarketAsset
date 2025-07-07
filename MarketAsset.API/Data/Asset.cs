using System.ComponentModel.DataAnnotations;

namespace MarketAsset.API.Data;

/// <summary>
/// Represents a market asset stored in the local database.
/// </summary>
public class Asset
{
    /// <summary>
    /// The unique identifier of the instrument (provided by the external API).
    /// Acts as the primary key.
    /// </summary>
    [Key]
    [Required]
    public string InstrumentId { get; set; } = default!;

    /// <summary>
    /// The trading symbol of the asset (e.g. "EUR/USD", "AAPL").
    /// </summary>
    [Required]
    public string Symbol { get; set; } = default!;

    /// <summary>
    /// The type or category of the asset (e.g. "forex", "stock", "crypto").
    /// </summary>
    [Required]
    public string Kind { get; set; } = default!;

    /// <summary>
    /// The data provider from which this asset's data is retrieved (e.g. "oanda", "simulation").
    /// </summary>
    [Required]
    public string Provider { get; set; } = default!;

    /// <summary>
    /// The most recently received price for this asset. Can be null if no price has been received yet.
    /// </summary>
    public decimal? LatestPrice { get; set; }

    /// <summary>
    /// The timestamp of the latest price update. Can be null if the price was never updated.
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}
