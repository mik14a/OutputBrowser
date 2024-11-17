using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.ViewModels;
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

    public static async Task SaveSettingsAsync() {
        var settings = GetService<IOptions<Models.OutputBrowserSettings>>().Value;
        var settingsJson = JsonSerializer.Serialize(settings, SerializerOptions);
        if (!Directory.Exists(PersonalDirectory)) Directory.CreateDirectory(PersonalDirectory);
        await File.WriteAllTextAsync(SettingsFile, settingsJson);
    }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        InitializeComponent();
#if !DEBUG
        CleanupExceptionLog();
        UnhandledException += AppUnhandledException;
#endif
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(SettingsFile, optional: true);
        builder.Services
            .Configure<Models.OutputBrowserSettings>(EnsureInitializeSettings)
            .Configure<Models.OutputBrowserSettings>(builder.Configuration)
            .AddSingleton<SettingViewModel>();
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
        var firstSelection = shell.NavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag.Equals("OutputPage"));
        shell.NavigationView.SelectedItem = firstSelection;
        _window.SetTitleBar(shell.AppTitleBar);
        _window.SetWindowSize(600, 800);
        _window.Activate();
    }

#if !DEBUG
    void CleanupExceptionLog() {
        var directory = new DirectoryInfo(PersonalDirectory);
        if (!directory.Exists) return;
        foreach (var file in directory.EnumerateFiles(string.Format(ExceptionFileFormat, "*"))) {
            file.Delete();
        }
    }
    void AppUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) {
        e.Handled = true;
        try {
            var text = $"Unhandled Exception: {e.Exception}\n\nStack Trace:\n{e.Exception.StackTrace}";
            var name = string.Format(ExceptionFileFormat, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"));
            var path = Path.Combine(PersonalDirectory, name);
            if (!Directory.Exists(PersonalDirectory)) Directory.CreateDirectory(PersonalDirectory);
            File.WriteAllText(path, text);
            System.Diagnostics.Debug.WriteLine($"Exception saved to: {path}");
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

    public static readonly string ExceptionFileFormat = "OutputBrowser.UnhandledException.{0}.log";
    public static readonly string PersonalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), nameof(OutputBrowser));
    public static readonly string SettingsFile = Path.Combine(PersonalDirectory, "Settings.json");
    public static readonly JsonSerializerOptions SerializerOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
}
