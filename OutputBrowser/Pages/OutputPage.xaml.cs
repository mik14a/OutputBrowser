using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.Extensions;
using OutputBrowser.Services;
using OutputBrowser.ViewModels;
using Windows.System;

namespace OutputBrowser.Pages;

/// <summary>
/// A output page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class OutputPage : Page, IDisposable
{
    [ObservableProperty]
    string _path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    [ObservableProperty]
    string _filters = "*.png";

    public ObservableCollection<OutputViewModel> Outputs { get; } = [];
    [ObservableProperty]
    bool _isScrolledAway;

    public OutputPage() {
        _settings = App.GetService<IOptions<Models.OutputBrowserSettings>>().Value.Default;

        _path = _settings.Path;
        _filters = _settings.Filters;
        _service = new FileSystemWatchService(_settings) {
            Filters = _filters
        };
        _service.Changed += OnChanged;
        _service.Deleted += OnDeleted;
        _service.Renamed += OnRenamed;
        Loaded += PageLoaded;
        Unloaded += PageUnloaded;

        InitializeComponent();
        DataContext = this;

        _Outputs.Loaded += (sender, args)
            => ((ListView)sender).GetScrollViewer().ViewChanged += (sender, e)
                => IsScrolledAway = ScrolledAway((ScrollViewer)sender);

        void PageLoaded(object sender, RoutedEventArgs e) {
            _Outputs.Focus(FocusState.Programmatic);
            IsScrolledAway = ScrolledAway(_Outputs.GetScrollViewer());
        }
        void PageUnloaded(object sender, RoutedEventArgs e) {
            _settings.Path = _path;
            _settings.Filters = _filters;
            App.SaveSettings();
        }
        static bool ScrolledAway(ScrollViewer viewer) => viewer.ScrollableHeight != viewer.VerticalOffset;
    }

    [RelayCommand]
    static async Task OpenWithDefaultAppAsync(OutputViewModel output) {
        await output.OpenWithDefaultAppAsync();
    }

    [RelayCommand]
    static async Task OpenFolderAsync(OutputViewModel output) {
        await output.OpenFolderAsync();
    }

    [RelayCommand]
    static async Task CopyAsync(OutputViewModel output) {
        await output.CopyAsync();
    }

    [RelayCommand]
    static async Task CopyImageAsync(OutputViewModel output) {
        await output.CopyImageAsync();
    }

    [RelayCommand]
    static void CopyPath(OutputViewModel output) {
        output.CopyPath();
    }

    [RelayCommand]
    static void CopyPrompt(OutputViewModel output) {
        output.CopyPrompt();
    }

    [RelayCommand]
    void ScrollToBottom() {
        _Outputs.ScrollToBottom(disableAnimation: false);
    }

    partial void OnPathChanged(string value) {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!Directory.Exists(value)) return;
        if (_service.Path == value) return;
        _service.Path = value;
    }

    partial void OnFiltersChanged(string value) {
        if (string.IsNullOrWhiteSpace(value)) return;
        _service.Filters = value;
    }

    void OnChanged(object sender, FileSystemEventArgs e) {
        DispatcherQueue.TryEnqueue(async () => {
            if (!File.Exists(e.FullPath)) return;
            if (Outputs.Any(o => o.ImagePath == e.FullPath)) return;
            var output = new OutputViewModel(Path, e.FullPath);
            var initialized = await output.InitializeAsync();
            if (initialized) Outputs.Add(output);
        });
    }

    void OnDeleted(object sender, FileSystemEventArgs e) {
        DispatcherQueue.TryEnqueue(() => {
            var viewModel = Outputs.FirstOrDefault(o => o.ImagePath == e.FullPath);
            if (viewModel != null) Outputs.Remove(viewModel);
        });
    }

    void OnRenamed(object sender, RenamedEventArgs e) {
        DispatcherQueue.TryEnqueue(async () => {
            var viewModel = Outputs.FirstOrDefault(o => o.ImagePath == e.OldFullPath);
            if (viewModel != null) Outputs.Remove(viewModel);
            if (!File.Exists(e.FullPath)) return;
            if (Outputs.Any(o => o.ImagePath == e.FullPath)) return;
            var output = new OutputViewModel(Path, e.FullPath);
            var initialized = await output.InitializeAsync();
            if (initialized) Outputs.Add(output);
        });
    }

    readonly FileSystemWatchService _service;
    readonly Models.WatchSettings _settings;

    public void Dispose() {
        ((IDisposable)_service).Dispose();
    }
}
