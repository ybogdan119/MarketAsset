using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents the mapping information for a specific provider, including provider-specific symbol and exchange details.
/// </summary>
public class MappingDto
{
    /// <summary>
    /// The symbol used by the specific data provider for this instrument.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// The exchange associated with the instrument, as defined by the provider.
    /// May be an empty string if not specified.
    /// </summary>
    [JsonPropertyName("exchange")]
    public string Exchange { get; set; } = string.Empty;
}
