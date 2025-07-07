using MarketAsset.API.Data;
using MarketAsset.API.DTO;

namespace MarketAsset.API.Services.IServices;

/// <summary>
/// Defines the contract for interacting with the Fintacharts API,
/// including authentication, asset retrieval, and historical price data.
/// </summary>
public interface IFintachartsService
{
    /// <summary>
    /// Retrieves an access token used to authenticate with the Fintacharts API.
    /// </summary>
    /// <returns>A JWT access token string.</returns>
    Task<string> GetAccessTokenAsync();

    /// <summary>
    /// Retrieves all available market assets from the Fintacharts API.
    /// </summary>
    /// <returns>A list of all available <see cref="Asset"/> entries.</returns>
    Task<List<Asset>> GetAllAssetsAsync();

    /// <summary>
    /// Retrieves historical price data for a specific instrument from the Fintacharts API.
    /// </summary>
    /// <param name="request">The parameters of the historical price request.</param>
    /// <returns>A list of historical price data points.</returns>
    Task<List<HistPriceData>> GetHistPriceAsync(HistPriceRequest request);
}
