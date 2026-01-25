using Avalonia;
using System;
using System.Threading.Tasks;

namespace MagoLauncher.Presentation;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Velopack startup handling (install/update hooks)
        Velopack.VelopackApp.Build().Run();

        LogTrace("Program.Main Started");

        // Add global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            LogException(error.ExceptionObject as Exception, "AppDomain.UnhandledException");
        };

        TaskScheduler.UnobservedTaskException += (sender, error) =>
        {
            LogException(error.Exception, "TaskScheduler.UnobservedTaskException");
            error.SetObserved();
        };

        try
        {
            LogTrace("Calling BuildAvaloniaApp");
            var builder = BuildAvaloniaApp();
            LogTrace("BuildAvaloniaApp returned, calling StartWithClassicDesktopLifetime");
            builder.StartWithClassicDesktopLifetime(args);
            LogTrace("StartWithClassicDesktopLifetime returned (App Exiting)");
        }
        catch (Exception e)
        {
            LogException(e, "Main Loop Exception");
        }
    }

    private static void LogTrace(string message)
    {
        try
        {
            System.IO.File.AppendAllText("startup_trace.txt", $"[{DateTime.Now}] {message}\n");
        }
        catch { }
    }

    private static void LogException(Exception? ex, string source)
    {
        if (ex == null) return;

        var message = $"[{DateTime.Now}] [{source}] Critical Error: {ex}\n\nStack Trace:\n{ex.StackTrace}\n\n";
        try
        {
            System.IO.File.AppendAllText("startup_crash.txt", message);
            Console.WriteLine(message);
        }
        catch
        {
            // If we can't write to file, at least try console
            Console.WriteLine($"FAILED TO LOG TO FILE: {ex}");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
