using MarketAsset.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketAsset.API.Controllers;

[Route("api/updater")]
[ApiController]
public class UpdaterController : ControllerBase
{
    private readonly AssetUpdaterControlService _control;
    private readonly ILogger<UpdaterController> _logger;

    public UpdaterController(AssetUpdaterControlService control, ILogger<UpdaterController> logger)
    {
        _control = control;
        _logger = logger;
    }

    [HttpPost("start")]
    public IActionResult Start()
    {
        _control.Start();
        _logger.LogInformation("Asset updater background service started.");
        return Ok(new { message = "Asset updater service started." });
    }

    [HttpPost("stop")]
    public IActionResult Stop()
    {
        _control.Stop();
        _logger.LogInformation("Asset updater background service stopped.");
        return Ok(new { message = "Asset updater service stopped." });
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new { running = _control.IsRunning });
    }
}
