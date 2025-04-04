using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
    public static Type Type => typeof(OutputPage);

    public ObservableCollection<OutputViewModel> Outputs { get; } = [];

    public bool IsDefault { get; } = false;

    [ObservableProperty]
    public partial string Path { get; set; }

    [ObservableProperty]
    public partial bool IsScrolledAway { get; set; }

    [ObservableProperty]
    public partial Symbol Icon { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    public OutputPage(WatchSettingsViewModel setting) {
        IsDefault = true;
        var service = new FileSystemWatchService("Default", setting.Path, setting.Filters);
        service.Changed += OnChanged;
        service.Deleted += OnDeleted;
        service.Renamed += OnRenamed;
        _services.Add(service);
        Loaded += OnPageLoaded;
        Path = setting.Path;
        InitializeComponent();
        DataContext = this;
        _Outputs.Loaded += OnOutputsLoaded;
    }

    public OutputPage(WatchesSettingViewModel watches) {
        Icon = watches.Icon;
        Title = watches.Name;
        watches.PropertyChanging += (s, e) => {
            if (e.PropertyName == nameof(watches.Icon)) {
                Icon = watches.Icon;
            } else if (e.PropertyName == nameof(watches.Name)) {
                Title = watches.Name;
            }
        };
        watches.Watches.CollectionChanged += OnWatchesCollectionChanged;
        watches.Watches.ForEach(AddWatch);
        Loaded += OnPageLoaded;
        InitializeComponent();
        DataContext = this;
        _Outputs.Loaded += OnOutputsLoaded;
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
        var service = _services.FirstOrDefault(s => s.Name == "Default");
        if (service == null) return;
        if (service.Path == value) return;
        service.Path = value;
    }

    void OnChanged(object sender, FileSystemEventArgs e) {
        if (sender is not FileSystemWatchService service) return;
        DispatcherQueue.TryEnqueue(async () => {
            Remove(Outputs, e.FullPath);
            if (!File.Exists(e.FullPath)) return;
            await AddAsync(Outputs, service.Name, _icons.TryGetValue(service.Name, out var icon) ? icon : null, service.Path, e.FullPath);
        });
    }

    void OnDeleted(object sender, FileSystemEventArgs e) {
        DispatcherQueue.TryEnqueue(() => Remove(Outputs, e.FullPath));
    }

    void OnRenamed(object sender, RenamedEventArgs e) {
        if (sender is not FileSystemWatchService service) return;
        DispatcherQueue.TryEnqueue(async () => {
            Remove(Outputs, e.OldFullPath);
            if (!File.Exists(e.FullPath)) return;
            await AddAsync(Outputs, service.Name, _icons.TryGetValue(service.Name, out var icon) ? icon : null, service.Path, e.FullPath);
        });
    }

    void OnPageLoaded(object sender, RoutedEventArgs e) {
        _Outputs.Focus(FocusState.Programmatic);
        IsScrolledAway = ScrolledAway(_Outputs.GetScrollViewer());
    }

    void OnOutputsLoaded(object sender, RoutedEventArgs e) {
        ((GridView)sender).GetScrollViewer().ViewChanged += OnOutputsViewChanged;
    }

    void OnOutputsViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
        IsScrolledAway = ScrolledAway((ScrollViewer)sender);
    }

    void OnWatchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.Action == NotifyCollectionChangedAction.Add) {
            e.NewItems.OfType<WatchSettingsViewModel>().ForEach(AddWatch);
        } else if (e.Action == NotifyCollectionChangedAction.Remove) {
            e.NewItems.OfType<WatchSettingsViewModel>().ForEach(RemoveWatch);
        }
    }

    void AddWatch(WatchSettingsViewModel watch) {
        var service = new FileSystemWatchService(watch.Name, watch.Path, watch.Filters);
        service.Changed += OnChanged;
        service.Deleted += OnDeleted;
        service.Renamed += OnRenamed;
        _services.Add(service);
        _icons.Add(watch.Name, watch.IconSource);
    }

    void RemoveWatch(WatchSettingsViewModel watch) {
        _services.Where(s => s.Name.Equals(watch.Name, StringComparison.InvariantCultureIgnoreCase))
            .Do(s => _services.Remove(s))
            .ForEach(s => ((IDisposable)s).Dispose());
    }

    readonly List<FileSystemWatchService> _services = [];
    readonly Dictionary<string, ImageSource> _icons = [];

    public void Dispose() {
        _services.ForEach(service => ((IDisposable)service).Dispose());
    }

    static bool ScrolledAway(ScrollViewer viewer) {
        return viewer.ScrollableHeight != viewer.VerticalOffset;
    }

    static void Remove(ObservableCollection<OutputViewModel> outputs, string fullPath) {
        var viewModel = outputs.FirstOrDefault(o => o.ImagePath == fullPath);
        if (viewModel != null) outputs.Remove(viewModel);
    }

    static async Task AddAsync(ObservableCollection<OutputViewModel> outputs, string sender, ImageSource icon, string basePath, string fullPath) {
        var output = new OutputViewModel(sender, icon, basePath, fullPath);
        var initialized = await output.InitializeAsync();
        if (initialized) outputs.Add(output);
    }
}
