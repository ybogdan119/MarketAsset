using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents a trading instrument retrieved from the external API (e.g. Fintacharts).
/// </summary>
public class InstrumentDto
{
    /// <summary>
    /// The unique identifier of the instrument.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The trading symbol of the instrument (e.g. "EUR/USD").
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// The type of asset (e.g. "forex", "stock", "crypto").
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// A dictionary of mappings per provider, where the key is the provider name (e.g. "oanda", "simulation")
    /// and the value is a detailed mapping object with provider-specific metadata.
    /// </summary>
    [JsonPropertyName("mappings")]
    public Dictionary<string, MappingDto> Mappings { get; set; } = new();
}
