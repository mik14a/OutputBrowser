using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace OutputBrowser;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static new App Current => (Application.Current as App)!;

    public static T GetService<T>() where T : class {
        var services = Current._host.Services;
        return services.GetService<T>() is T service
            ? service
            : throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
    }

    public static void SaveSettings() {
        var settings = GetService<IOptions<Models.OutputBrowserSettings>>().Value;
        var settingsJson = JsonSerializer.Serialize(settings, SerializerOptions);
        if (!Directory.Exists(PersonalDirectory)) Directory.CreateDirectory(PersonalDirectory);
        File.WriteAllText(SettingsFile, settingsJson);
    }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        InitializeComponent();
#if !DEBUG
        UnhandledException += AppUnhandledException;
#endif

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(SettingsFile, optional: true);
        builder.Services.Configure<Models.OutputBrowserSettings>(EnsureInitializeSettings);
        builder.Services.Configure<Models.OutputBrowserSettings>(builder.Configuration);
        _host = builder.Build();
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

    static void EnsureInitializeSettings(Models.OutputBrowserSettings settings) {
        settings.Default ??= new Models.WatchSettings {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            Filters = "*.png;*.jpg;*.jpeg"
        };
    }

    readonly IHost _host;

    MainWindow _window;

    public static readonly string PersonalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), nameof(OutputBrowser));
    public static readonly string SettingsFile = Path.Combine(PersonalDirectory, "Settings.json");
    public static readonly JsonSerializerOptions SerializerOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
}
