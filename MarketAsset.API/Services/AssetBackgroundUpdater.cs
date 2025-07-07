using MarketAsset.API.Configuration;
using MarketAsset.API.Data;
using MarketAsset.API.Services.IServices;
using Microsoft.Extensions.Options;

namespace MarketAsset.API.Services;

/// <summary>
/// A background service that periodically synchronizes market assets from the external API
/// and stores/updates them in the local database.
/// </summary>
public class AssetBackgroundUpdater : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AssetBackgroundUpdater> _logger;
    private readonly TimeSpan _updateInterval;
    private readonly TimeSpan _sleepInterval;
    private readonly AssetUpdaterControlService _control;
    private readonly BackgroundsServicesOptions _options;
    private readonly IFintachartsService _fintaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetBackgroundUpdater"/> class.
    /// </summary>
    public AssetBackgroundUpdater(
        IServiceProvider serviceProvider,
        ILogger<AssetBackgroundUpdater> logger,
        AssetUpdaterControlService control,
        IOptions<BackgroundsServicesOptions> options,
        IFintachartsService fintaService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
        _updateInterval = TimeSpan.FromSeconds(_options.UpdateAssetsIntervalSeconds);
        _sleepInterval = TimeSpan.FromSeconds(_options.SleepIntervalSeconds);
        _control = control;
        _fintaService = fintaService;
    }

    /// <summary>
    /// Executes the background update loop. 
    /// Periodically fetches all assets from the Fintacharts API, compares them to local data,
    /// and performs insert or update operations accordingly.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel background execution.</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Asset updater background service started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            // Skip update if the updater is paused via control service
            if (_control.IsRunning == false)
            {
                await Task.Delay(_sleepInterval, cancellationToken);
                continue;
            }

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var assets = await _fintaService.GetAllAssetsAsync();

            int added = 0;
            int updated = 0;

            foreach (var asset in assets)
            {
                var existing = await db.Assets.FindAsync(asset.InstrumentId);

                if (existing == null)
                {
                    db.Assets.Add(asset);
                    added++;
                }
                else
                {
                    if (existing.Symbol != asset.Symbol ||
                        existing.Kind != asset.Kind ||
                        existing.Provider != asset.Provider)
                    {
                        existing.Symbol = asset.Symbol;
                        existing.Kind = asset.Kind;
                        existing.Provider = asset.Provider;
                        updated++;
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset sync complete. Added: {added}, Updated: {updated}", added, updated);

            await Task.Delay(_updateInterval, cancellationToken);
        }
    }
}
