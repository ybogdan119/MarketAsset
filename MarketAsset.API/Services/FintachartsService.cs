using MarketAsset.API.Configuration;
using MarketAsset.API.Data;
using MarketAsset.API.DTO;
using MarketAsset.API.Services.IServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MarketAsset.API.Services;

/// <summary>
/// Provides methods to interact with the Fintacharts API, including authentication,
/// instrument retrieval, and historical price data fetching.
/// </summary>
public class FintachartsService : IFintachartsService
{
    private readonly HttpClient _httpClient;
    private readonly FintachartsOptions _options;
    private readonly ILogger<FintachartsService> _logger;
    private readonly IMemoryCache _memoryCache;

    private const string TokenCacheKey = "FintachartsAccessToken";

    /// <summary>
    /// Initializes a new instance of the <see cref="FintachartsService"/> class.
    /// </summary>
    public FintachartsService(
        IHttpClientFactory httpClientFactory,
        IOptions<FintachartsOptions> options,
        ILogger<FintachartsService> logger,
        IMemoryCache memoryCache)
    {
        _httpClient = httpClientFactory.CreateClient();
        _options = options.Value;
        _logger = logger;
        _memoryCache = memoryCache;

        ValidateConfguration();
    }

    /// <summary>
    /// Validates that all required Fintacharts configuration values are set.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if any required setting is missing.</exception>
    private void ValidateConfguration()
    {
        if (string.IsNullOrEmpty(_options.BaseUrl))
            throw new InvalidOperationException("Fintacharts BaseUrl is not configured.");
        if (string.IsNullOrEmpty(_options.TokenEndpoint))
            throw new InvalidOperationException("Fintacharts TokenEndpoint is not configured.");
        if (string.IsNullOrEmpty(_options.InstrumentsEndpoint))
            throw new InvalidOperationException("Fintacharts InstrumentsEndpoint is not configured.");
        if (string.IsNullOrEmpty(_options.HistoryEndpoint))
            throw new InvalidOperationException("Fintacharts HistoryEndpoint is not configured.");
        if (string.IsNullOrEmpty(_options.ClientId))
            throw new InvalidOperationException("Fintacharts ClientId is not configured.");
        if (string.IsNullOrEmpty(_options.Username))
            throw new InvalidOperationException("Fintacharts Username is not configured.");
        if (string.IsNullOrEmpty(_options.Password))
            throw new InvalidOperationException("Fintacharts Password is not configured.");
    }

    /// <summary>
    /// Retrieves a valid access token from the Fintacharts API, using in-memory cache when available.
    /// </summary>
    /// <returns>The access token string.</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        if (_memoryCache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogInformation("Using cached token from memory cache");
            return cachedToken;
        }

        var url = $"{_options.BaseUrl}{_options.TokenEndpoint}";
        _logger.LogInformation("Requesting new access token from {Url}", url);

        try
        {
            var form = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _options.ClientId },
                { "username", _options.Username },
                { "password", _options.Password }
            };

            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get access token. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to retrieve access token. Status: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponse>(json);

            if (token == null || string.IsNullOrEmpty(token.AccessToken))
            {
                _logger.LogError("Invalid token response: {Json}", json);
                throw new InvalidOperationException("Failed to retrieve a valid access token.");
            }

            var expirationBuffer = Math.Min(token.ExpiresIn / 10, 60);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(token.ExpiresIn - expirationBuffer),
                Priority = CacheItemPriority.High
            };

            _memoryCache.Set(TokenCacheKey, token.AccessToken, cacheOptions);
            _logger.LogDebug("Access token retrieved successfully.");

            return token.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while getting access token");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout occurred while getting access token");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all available assets from the Fintacharts API, handling pagination.
    /// </summary>
    /// <returns>A list of all market assets.</returns>
    public async Task<List<Asset>> GetAllAssetsAsync()
    {
        var assets = new List<Asset>();
        int page = 1;

        _logger.LogInformation("Starting to fetch all assets.");

        try
        {
            while (true)
            {
                var url = $"{_options.BaseUrl}{_options.InstrumentsEndpoint}?page={page}&size={_options.PageSize}";
                var json = await CallFintachartsAsync(url);

                if (string.IsNullOrEmpty(json))
                    break;

                var data = JsonSerializer.Deserialize<InstrumentResponse>(json);

                if (data == null || data.Data == null || data.Data.Count == 0)
                    break;

                foreach (var item in data.Data)
                {
                    if (item.Mappings.Count == 0)
                        continue;

                    var provider = item.Mappings.Keys.First();

                    assets.Add(new Asset
                    {
                        InstrumentId = item.Id,
                        Symbol = item.Symbol,
                        Kind = item.Kind,
                        Provider = provider
                    });
                }

                if (page >= data.Paging.Pages)
                    break;

                page++;
            }

            _logger.LogInformation("Successfully fetched {Count} assets in total", assets.Count);
            return assets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching assets");
            throw;
        }
    }

    /// <summary>
    /// Retrieves historical price data for the specified instrument and time range.
    /// </summary>
    /// <param name="request">The historical price request parameters.</param>
    /// <returns>A list of historical price data points.</returns>
    public async Task<List<HistPriceData>> GetHistPriceAsync(HistPriceRequest request)
    {
        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["instrumentId"] = request.InstrumentId,
                ["provider"] = request.Provider,
                ["interval"] = request.Interval.ToString(),
                ["periodicity"] = request.Periodicity,
                ["startDate"] = request.StartDate.ToString("yyyy-MM-dd"),
                ["endDate"] = request.EndDate.ToString("yyyy-MM-dd")
            };

            var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = $"{_options.BaseUrl}{_options.HistoryEndpoint}?{query}";

            _logger.LogDebug("Fetching price history.");

            string json = await CallFintachartsAsync(url);

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("Empty response received for price history request");
                return new List<HistPriceData>();
            }

            var data = JsonSerializer.Deserialize<HistPriceResponse>(json);
            var result = data?.Data ?? new List<HistPriceData>();

            _logger.LogDebug("Successfully retrieved {Count} price records", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching price history");
            throw;
        }
    }

    /// <summary>
    /// Executes an authenticated GET request to the Fintacharts API.
    /// </summary>
    /// <param name="url">The full URL of the API endpoint.</param>
    /// <returns>The raw JSON response body.</returns>
    private async Task<string> CallFintachartsAsync(string url)
    {
        try
        {
            string token = await GetAccessTokenAsync();

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API request failed. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while calling Fintacharts API: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout occurred while calling Fintacharts API: {Url}", url);
            throw;
        }
    }
}
