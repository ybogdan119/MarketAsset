using MarketAsset.API.Data;
using MarketAsset.API.DTO;
using MarketAsset.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAsset.API.Controllers;

[Route("api/assets")]
[ApiController]
public class AssetsController : ControllerBase
{
    private readonly IFintachartsService _fintachartsService;
    private readonly AppDbContext _db;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IFintachartsService fintachartsService, AppDbContext db, ILogger<AssetsController> logger)
    {
        _fintachartsService = fintachartsService;
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetAssetsFromDatabase()
    {
        try
        {
            var assets = await _db.Assets
                .Select(a => a.Symbol)
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} assets from database", assets.Count);
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving assets");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<HistPriceData>>> GetHistPrice(
        [FromQuery] string symbol = "EUR/USD",
        [FromQuery] string intervalStr = "1",
        [FromQuery] string periodicity = "year",
        [FromQuery] DateTime startDate = default,
        [FromQuery] DateTime endDate = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            _logger.LogWarning("GetHistPrice called with empty symbol");
            return BadRequest("Symbol cannot be empty.");
        }

        if (!int.TryParse(intervalStr, out int interval))
        {
            _logger.LogWarning("GetHistPrice called with invalid interval");
            return BadRequest("Interval must be a valid integer.");
        }

        if (interval <= 0)
        {
            _logger.LogWarning("GetHistPrice called with invalid interval: {Interval}", interval);
            return BadRequest("Interval must be greater than zero.");
        }

        if (startDate != default && endDate != default && startDate > endDate)
        {
            _logger.LogWarning("GetHistPrice called with invalid date range: {StartDate} > {EndDate}", startDate, endDate);
            return BadRequest("Start date cannot be greater than end date.");
        }

        try
        {
            var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);

            if (asset is null)
            {
                _logger.LogWarning("Symbol '{Symbol}' not found in database", symbol);
                return NotFound($"Symbol with '{symbol}' not found.");
            }

            var request = new HistPriceRequest
            {
                InstrumentId = asset.InstrumentId,
                Provider = asset.Provider,
                Interval = interval,
                Periodicity = periodicity,
                StartDate = startDate,
                EndDate = endDate == default ? DateTime.UtcNow : endDate
            };

            _logger.LogInformation("Requesting price history for asset {Symbol} from {StartDate} to {EndDate}",
               symbol, request.StartDate, request.EndDate);

            var priceHist = await _fintachartsService.GetHistPriceAsync(request);

            if (priceHist is null || priceHist.Count == 0)
            {
                _logger.LogWarning("No price history found for asset {Symbol} with specified parameters", symbol);
                return NotFound($"No price history found for asset '{symbol}' with the specified parameters.");
            }

            _logger.LogInformation("Successfully retrieved {Count} price records for asset {Symbol}",priceHist.Count, symbol);

            return Ok(priceHist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting price history for asset {Symbol}", symbol);
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpGet("price")]
    public async Task<ActionResult<List<AssetPriceDto>>> GetPrice([FromQuery] string symbols)
    {
        if (string.IsNullOrWhiteSpace(symbols))
        {
            return BadRequest("Symbols parameter is required");
        }

        var requestedSymbols = symbols
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToUpper())
            .ToList();

        try
        {
            var foundAssets = await _db.Assets
                .Where(a => requestedSymbols.Contains(a.Symbol.ToUpper()))
                .ToListAsync();

            var result = foundAssets.Select(a => new AssetPriceDto
            {
                Symbol = a.Symbol,
                LatestPrice = a.LatestPrice,
                LastUpdated = a.LastUpdated
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving assets");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpGet("price/all")]
    public async Task<ActionResult<List<AssetPriceDto>>> GetAllPrices()
    {
        try
        {
            var result = await _db.Assets.Select(a => new AssetPriceDto
            {
                Symbol = a.Symbol,
                LatestPrice = a.LatestPrice,
                LastUpdated = a.LastUpdated
            })
                .OrderByDescending(a => a.LastUpdated)
                .ToListAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving assets");
            return StatusCode(500, "An unexpected error occurred");
        }
    }
}
