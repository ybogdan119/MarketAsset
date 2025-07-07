namespace MarketAsset.API.DTO;

/// <summary>
/// Represents a request to retrieve historical price data for a specific instrument.
/// </summary>
public class HistPriceRequest
{
    /// <summary>
    /// The unique identifier of the instrument (e.g., asset ID in the Fintacharts system).
    /// </summary>
    public string InstrumentId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the data provider (e.g., "oanda", "simulation", "alpaca").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// The interval value for the price bars (e.g., 1 for one-minute bars).
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// The periodicity type (e.g., "minute", "hour", "day").
    /// </summary>
    public string Periodicity { get; set; } = string.Empty;

    /// <summary>
    /// The start date of the requested historical data range (inclusive).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the requested historical data range (inclusive).
    /// </summary>
    public DateTime EndDate { get; set; }
}
