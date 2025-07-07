using System.Text.Json.Serialization;

namespace MarketAsset.API.DTO;

/// <summary>
/// Represents the API response that contains a list of instruments along with paging information.
/// </summary>
public class InstrumentResponse
{
    /// <summary>
    /// Pagination information for the response, such as current page, total pages, and total items.
    /// </summary>
    [JsonPropertyName("paging")]
    public PagingInfo Paging { get; set; } = new();

    /// <summary>
    /// A list of instruments returned by the API.
    /// </summary>
    [JsonPropertyName("data")]
    public List<InstrumentDto> Data { get; set; } = new();
}
