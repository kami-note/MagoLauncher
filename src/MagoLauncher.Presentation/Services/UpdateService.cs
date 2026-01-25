using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace MagoLauncher.Presentation.Services;

public class UpdateService
{
    // TODO: UPDATE THIS URL to your actual release hosting location.
    // If using GitHub Releases, replace SimpleWebSource with GithubSource.
    // Example: new GithubSource("https://github.com/YourUsername/MagoLauncher", null, false);
    private const string UpdateSourceUrl = "https://mago-launcher-server.vercel.app/releases";

    public async Task CheckAndApplyUpdatesAsync()
    {
        try
        {
            // Initialize the UpdateManager
            // Note: UpdateManager works best when the app is installed/packaged via Velopack (vpk).
            var mgr = new UpdateManager(new SimpleWebSource(UpdateSourceUrl));

            if (!mgr.IsInstalled)
            {
                // When running locally (F5), IsInstalled is usually false.
                System.Diagnostics.Debug.WriteLine("[UpdateService] App not installed via Velopack. Skipping update check.");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[UpdateService] Checking for updates...");
            var newVersion = await mgr.CheckForUpdatesAsync();

            if (newVersion == null)
            {
                System.Diagnostics.Debug.WriteLine("[UpdateService] No updates available.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[UpdateService] Downloading update: {newVersion.TargetFullRelease.Version}");
            await mgr.DownloadUpdatesAsync(newVersion);

            System.Diagnostics.Debug.WriteLine("[UpdateService] Restarting to apply update...");
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateService] Error checking/applying updates: {ex}");
            // Consider logging this to a file or showing a user notification
        }
    }
}
