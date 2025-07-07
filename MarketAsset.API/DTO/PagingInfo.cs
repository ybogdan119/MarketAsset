using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents pagination metadata returned by the API for paged results.
/// </summary>
public class PagingInfo
{
    /// <summary>
    /// The current page number in the paged result set (1-based index).
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// The total number of available pages.
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    [JsonPropertyName("items")]
    public int Items { get; set; }
}
