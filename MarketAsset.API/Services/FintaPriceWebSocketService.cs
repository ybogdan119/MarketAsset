using MarketAsset.API.Configuration;
using MarketAsset.API.Data;
using MarketAsset.API.DTO;
using MarketAsset.API.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MarketAsset.API.Services;

/// <summary>
/// Background service that connects to the Fintacharts WebSocket API and subscribes to real-time price updates
/// for all assets grouped by provider. It updates the local database with the latest price data.
/// </summary>
public class FintaPriceWebSocketService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IFintachartsService _fintaService;
    private readonly BackgroundsServicesOptions _options;
    private readonly ILogger<FintaPriceWebSocketService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FintaPriceWebSocketService"/> class.
    /// </summary>
    public FintaPriceWebSocketService(
        IServiceScopeFactory scopeFactory,
        IFintachartsService _fintaService,
        IOptions<BackgroundsServicesOptions> options,
        ILogger<FintaPriceWebSocketService> logger)
    {
        _scopeFactory = scopeFactory;
        this._fintaService = _fintaService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Entry point for the background service. Connects to the WebSocket server for each provider
    /// and starts receiving real-time price updates for all known assets.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(_options.WsDelaySeconds), stoppingToken); // Wait for the app to fully start
        _logger.LogInformation("Starting WebSocket price service");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var token = await _fintaService.GetAccessTokenAsync();

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var allAssets = await db.Assets.ToListAsync(stoppingToken);

                if (allAssets.Count == 0)
                {
                    _logger.LogWarning("No assets found in database. Waiting before retry...");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                var assetsByProvider = allAssets
                    .GroupBy(a => a.Provider)
                    .ToList();

                _logger.LogInformation("Starting WebSocket connections for {ProviderCount} providers with {AssetCount} total assets",
                    assetsByProvider.Count, allAssets.Count);

                var tasks = assetsByProvider.Select(group =>
                    RunProviderWebSocketAsync(group.Key, group.ToList(), token, stoppingToken)
                ).ToArray();

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WebSocket service main loop");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Establishes a WebSocket connection for a specific provider and subscribes to all associated assets.
    /// Processes incoming price update messages in a loop.
    /// </summary>
    private async Task RunProviderWebSocketAsync(string provider, List<Asset> assets, string token, CancellationToken stoppingToken)
    {
        ClientWebSocket? ws = null;

        try
        {
            ws = new ClientWebSocket();
            var wsUrl = new Uri($"{_options.WebSocketUrl}?token={token}");

            _logger.LogInformation("Connecting to WebSocket for provider {Provider} with {AssetCount} assets",
                provider, assets.Count);

            await ws.ConnectAsync(wsUrl, stoppingToken);
            _logger.LogInformation("WebSocket connected for provider {Provider}", provider);

            // Subscribe to all assets for the provider
            foreach (var asset in assets)
            {
                try
                {
                    var subMsg = new
                    {
                        type = "l1-subscription",
                        id = Guid.NewGuid().ToString(),
                        instrumentId = asset.InstrumentId,
                        provider,
                        subscribe = true,
                        kinds = new[] { "last" }
                    };

                    var json = JsonSerializer.Serialize(subMsg);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error subscribing to asset {AssetId} for provider {Provider}",
                        asset.InstrumentId, provider);
                }
            }

            _logger.LogInformation("Subscribed to {Count} assets for provider {Provider}", assets.Count, provider);

            var buffer = new byte[8192];

            while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogInformation("WebSocket close requested for provider {Provider}", provider);
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", stoppingToken);
                            return;
                        }

                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(ms, Encoding.UTF8);
                    var json = await reader.ReadToEndAsync();

                    await ProcessWebSocketMessage(json, provider, stoppingToken);
                }
                catch (WebSocketException ex)
                {
                    _logger.LogError(ex, "WebSocket error for provider {Provider}", provider);
                    break;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("WebSocket operation cancelled for provider {Provider}", provider);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing WebSocket message for provider {Provider}", provider);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket connection for provider {Provider}", provider);
        }
        finally
        {
            if (ws != null)
            {
                try
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing WebSocket for provider {Provider}", provider);
                }

                ws.Dispose();
            }

            _logger.LogInformation("WebSocket connection closed for provider {Provider}", provider);
        }
    }

    /// <summary>
    /// Parses and processes an incoming WebSocket message.
    /// If the message is a valid price update, updates the asset record in the database.
    /// </summary>
    private async Task ProcessWebSocketMessage(string json, string provider, CancellationToken stoppingToken)
    {
        try
        {
            var message = JsonSerializer.Deserialize<WsPriceMsg>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message?.Type == "l1-update" && message.Last is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var asset = await db.Assets
                    .FirstOrDefaultAsync(a => a.InstrumentId == message.InstrumentId, stoppingToken);

                if (asset is not null)
                {
                    asset.LatestPrice = message.Last.Price;
                    asset.LastUpdated = message.Last.Timestamp;
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogDebug("Updated price for {Symbol} ({Provider}): {Price} @ {Timestamp}",
                        asset.Symbol, provider, message.Last.Price, message.Last.Timestamp);
                }
                else
                {
                    _logger.LogWarning("Asset with InstrumentId {InstrumentId} not found for provider {Provider}",
                        message.InstrumentId, provider);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing WebSocket message for provider {Provider}: {Json}", provider, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message for provider {Provider}", provider);
        }
    }
}
