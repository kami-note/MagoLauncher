using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace MagoLauncher.Presentation.Services;

public class UpdateService
{
    private readonly LogService _logService;
    // TODO: UPDATE THIS URL to your actual release hosting location.
    // If using GitHub Releases, replace SimpleWebSource with GithubSource.
    // Example: new GithubSource("https://github.com/YourUsername/MagoLauncher", null, false);
    private const string UpdateSourceUrl = "https://mago-launcher-server.vercel.app/releases";

    public UpdateService(LogService logService)
    {
        _logService = logService;
    }

    public async Task CheckAndApplyUpdatesAsync()
    {
        try
        {
            _logService.Log($"[UpdateService] Initializing. Source: {UpdateSourceUrl}");

            // Initialize the UpdateManager
            // Note: UpdateManager works best when the app is installed/packaged via Velopack (vpk).
            var mgr = new UpdateManager(new SimpleWebSource(UpdateSourceUrl));

            if (!mgr.IsInstalled)
            {
                // When running locally (F5), IsInstalled is usually false.
                _logService.Log("[UpdateService] App not installed via Velopack (IsInstalled = false). Skipping update check.");
                return;
            }

            _logService.Log("[UpdateService] Checking for updates...");
            var newVersion = await mgr.CheckForUpdatesAsync();

            if (newVersion == null)
            {
                _logService.Log("[UpdateService] No updates available.");
                return;
            }

            _logService.Log($"[UpdateService] Downloading update: {newVersion.TargetFullRelease.Version}");
            await mgr.DownloadUpdatesAsync(newVersion);

            _logService.Log("[UpdateService] Restarting to apply update...");
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
        catch (Exception ex)
        {
            _logService.Error("[UpdateService] Error checking/applying updates", ex);
            // Consider logging this to a file or showing a user notification
        }
    }
}
