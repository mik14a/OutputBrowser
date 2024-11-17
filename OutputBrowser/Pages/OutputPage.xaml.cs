using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public ObservableCollection<OutputViewModel> Outputs { get; } = [];

    [ObservableProperty] string _path;
    [ObservableProperty] bool _isScrolledAway;

    public OutputPage() {
        _setting = App.GetService<SettingViewModel>();

        _path = _setting.Path;
        _service = new FileSystemWatchService(_setting.Path, _setting.Filters);
        _service.Changed += OnChanged;
        _service.Deleted += OnDeleted;
        _service.Renamed += OnRenamed;
        Loaded += PageLoaded;

        InitializeComponent();
        DataContext = this;

        _Outputs.Loaded += (sender, args)
            => ((ListView)sender).GetScrollViewer().ViewChanged += (sender, e)
                => IsScrolledAway = ScrolledAway((ScrollViewer)sender);

        void PageLoaded(object sender, RoutedEventArgs e) {
            _Outputs.Focus(FocusState.Programmatic);
            IsScrolledAway = ScrolledAway(_Outputs.GetScrollViewer());
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

    void OnChanged(object sender, FileSystemEventArgs e) {
        DispatcherQueue.TryEnqueue(async () => {
            Remove(Outputs, e.FullPath);
            if (!File.Exists(e.FullPath)) return;
            await AddAsync(Outputs, Path, e.FullPath);
        });
    }

    void OnDeleted(object sender, FileSystemEventArgs e) {
        DispatcherQueue.TryEnqueue(() => Remove(Outputs, e.FullPath));
    }

    void OnRenamed(object sender, RenamedEventArgs e) {
        DispatcherQueue.TryEnqueue(async () => {
            Remove(Outputs, e.OldFullPath);
            if (!File.Exists(e.FullPath)) return;
            await AddAsync(Outputs, Path, e.FullPath);
        });
    }

    readonly FileSystemWatchService _service;
    readonly SettingViewModel _setting;

    public void Dispose() {
        ((IDisposable)_service).Dispose();
    }

    static void Remove(ObservableCollection<OutputViewModel> outputs, string fullPath) {
        var viewModel = outputs.FirstOrDefault(o => o.ImagePath == fullPath);
        if (viewModel != null) outputs.Remove(viewModel);
    }

    static async Task AddAsync(ObservableCollection<OutputViewModel> outputs, string basePath, string fullPath) {
        var output = new OutputViewModel(basePath, fullPath);
        var initialized = await output.InitializeAsync();
        if (initialized) outputs.Add(output);
    }
}
