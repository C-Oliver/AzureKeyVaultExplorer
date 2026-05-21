using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;

namespace VaultExplorer.UITests;

/// <summary>
/// Shared fixture that launches the application once for the entire test collection.
/// Implements IDisposable to close the app when all tests in the collection are done.
/// </summary>
public sealed class AppFixture : IDisposable
{
    public Application App { get; }
    public UIA3Automation Automation { get; }
    public Window MainWindow { get; }

    private static readonly string ExePath = Path.Combine(
        FindSolutionRoot(), "Vault", "Explorer", "bin", "Debug", "net10.0-windows", "VaultExplorer.exe");

    public AppFixture()
    {
        if (!File.Exists(ExePath))
        {
            // Build the app if not already built
            var build = Process.Start(new ProcessStartInfo("dotnet", $"build \"{Path.Combine(FindSolutionRoot(), "Vault", "Explorer", "VaultExplorer.csproj")}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = FindSolutionRoot()
            })!;
            build.WaitForExit(120_000);
        }

        Automation = new UIA3Automation();
        App = Application.Launch(ExePath);

        // Wait for the main window to appear
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(15));
    }

    public void Dispose()
    {
        App?.Close();
        App?.Dispose();
        Automation?.Dispose();
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "AzureKeyVaultExplorer.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new InvalidOperationException("Could not find solution root");
    }
}

[CollectionDefinition("App")]
public class AppCollection : ICollectionFixture<AppFixture> { }
