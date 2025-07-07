namespace MarketAsset.API.Services;

/// <summary>
/// A simple service that controls whether the <see cref="AssetBackgroundUpdater"/> is active.
/// Used to start or stop background synchronization dynamically (e.g. via an API call).
/// </summary>
public class AssetUpdaterControlService
{
    private bool _isRunning = true;

    /// <summary>
    /// Gets a value indicating whether the asset updater is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Starts the asset updater background process.
    /// </summary>
    public void Start() => _isRunning = true;

    /// <summary>
    /// Stops the asset updater background process.
    /// </summary>
    public void Stop() => _isRunning = false;
}
