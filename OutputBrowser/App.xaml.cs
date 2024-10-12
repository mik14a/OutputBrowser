using System;
using System.IO;
using Microsoft.UI.Xaml;
using WinUIEx;

#if !DEBUG
using System;
using System.IO;
#endif

namespace OutputBrowser;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static readonly string SettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".OutputBrowser");

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        InitializeComponent();
#if !DEBUG
        UnhandledException += AppUnhandledException;
#endif
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
        _window = new MainWindow {
            MinWidth = 320, MinHeight = 240,
            Content = new Pages.ShellPage(),
            ExtendsContentIntoTitleBar = true
        };
        var shell = (Pages.ShellPage)_window.Content;
        _window.SetTitleBar(shell.AppTitleBar);
        _window.SetWindowSize(600, 800);
        _window.Activate();
    }

#if !DEBUG
    void AppUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) {
        e.Handled = true;
        try {
            var exceptionText = $"Unhandled Exception: {e.Exception}\n\nStack Trace:\n{e.Exception.StackTrace}";
            var fileName = $"OutputBrowser.UnhandledException.{DateTime.Now:yyyyMMdd_HHmmssfff}.txt";
            var documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(documentFolder, fileName);
            File.WriteAllText(filePath, exceptionText);
            System.Diagnostics.Debug.WriteLine($"Exception saved to: {filePath}");
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Failed to save exception: {ex}");
        }
    }
#endif

    MainWindow _window;
}
