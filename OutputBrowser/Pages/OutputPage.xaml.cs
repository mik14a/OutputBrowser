using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using OutputBrowser.Extensions;
using OutputBrowser.Helpers;
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

    public OutputPage(WatchesSettingsViewModel watches) {
        Icon = watches.Icon;
        Title = watches.Name;
        watches.PropertyChanged += OnWatchesSettingsPropertyChanged;
        watches.Watches.CollectionChanged += OnWatchesCollectionChanged;
        watches.Watches.ForEach(AddWatch);
        Loaded += OnPageLoaded;
        InitializeComponent();
        DataContext = this;
        _Outputs.Loaded += OnOutputsLoaded;
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
            _watches.TryGetValue(service.Name, out var watch);
            await AddAsync(Outputs, service.Name, watch, service.Path, e.FullPath);
            if (watch?.Notification ?? false) Notification(service, watch, e.FullPath);
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
            _watches.TryGetValue(service.Name, out var watch);
            await AddAsync(Outputs, service.Name, watch, service.Path, e.FullPath);
            if (watch?.Notification ?? false) Notification(service, watch, e.FullPath);
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
            e.OldItems.OfType<WatchSettingsViewModel>().ForEach(RemoveWatch);
        }
    }

    void OnWatchesSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (sender is not WatchesSettingsViewModel watches) return;
        if (e.PropertyName == nameof(watches.Icon)) {
            Icon = watches.Icon;
        } else if (e.PropertyName == nameof(watches.Name)) {
            Title = watches.Name;
            _watches.Values.Where(w => w.Notification)
                           .Where(w => w.IconSource != null)
                           .ForEach(StoreIcon);
        }
    }

    void OnWatchSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (sender is not WatchSettingsViewModel watch) return;
        var service = _services.FirstOrDefault(s => s.Name.Equals(watch.Name, StringComparison.InvariantCultureIgnoreCase));
        if (service == null) return;

        if (e.PropertyName == nameof(watch.Icon)) {
            if (watch.Notification) StoreIcon(watch);
        } else if (e.PropertyName == nameof(watch.Name)) {
            // Can not change name
        } else if (e.PropertyName == nameof(watch.Path)) {
            service.Path = watch.Path;
        } else if (e.PropertyName == nameof(watch.Filters)) {
            service.Filters = watch.Filters;
        } else if (e.PropertyName == nameof(watch.Format)) {
            service.Format = watch.Format;
        } else if (e.PropertyName == nameof(watch.Notification)) {
            if (watch.Notification && watch.IconSource != null) StoreIcon(watch);
        }
    }

    void AddWatch(WatchSettingsViewModel watch) {
        var service = new FileSystemWatchService(watch.Name, watch.Path, watch.Filters) {
            Format = watch.Format
        };
        service.Changed += OnChanged;
        service.Deleted += OnDeleted;
        service.Renamed += OnRenamed;
        _services.Add(service);
        watch.PropertyChanged += OnWatchSettingsPropertyChanged;
        if (watch.Notification && watch.IconSource != null) StoreIcon(watch);
        _watches.Add(watch.Name, watch);
    }

    void RemoveWatch(WatchSettingsViewModel watch) {
        var services = _services.Where(s => s.Name.Equals(watch.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        services.ForEach(s => {
            _services.Remove(s);
            ((IDisposable)s).Dispose();
        });
        if (_watches.Remove(watch.Name)) {
            watch.PropertyChanged -= OnWatchSettingsPropertyChanged;
        }
    }

    void StoreIcon(WatchSettingsViewModel watch) {
        var folderName = System.IO.Path.Combine(_applicationData, nameof(OutputBrowser));
        var fileName = watch.IconFileName;
        _ = ImageHelper.SaveToFileAsync(watch.IconSource, folderName, fileName);
    }

    readonly List<FileSystemWatchService> _services = [];
    readonly Dictionary<string, WatchSettingsViewModel> _watches = [];

    static OutputPage() {
        _applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var roamingFolder = System.IO.Path.Combine(_applicationData, nameof(OutputBrowser));
        if (!Directory.Exists(roamingFolder)) {
            Directory.CreateDirectory(roamingFolder);
        }
    }

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

    static async Task AddAsync(ObservableCollection<OutputViewModel> outputs, string sender, WatchSettingsViewModel watch, string basePath, string fullPath) {
        var output = new OutputViewModel(sender, watch?.IconSource, watch?.Format, basePath, fullPath);
        var initialized = await output.InitializeAsync();
        if (initialized) outputs.Add(output);
    }

    static void Notification(FileSystemWatchService service, WatchSettingsViewModel watch, string fullPath) {
        var relativePath = System.IO.Path.GetRelativePath(service.Path, fullPath);
        var builder = new AppNotificationBuilder();
        var iconUri = watch.IconSource != null
            ? new Uri($"file://{_applicationData}/{nameof(OutputBrowser)}/{watch.IconFileName}")
            : new Uri($"ms-appx:///Assets/Titles/TitlebarLogo.png");
        var imageUri = new Uri($"file://{fullPath}");

        var message = string.IsNullOrWhiteSpace(watch.Format)
            ? relativePath
            : watch.Format
                .Replace("{{name}}", System.IO.Path.GetFileName(fullPath))
                .Replace("{{path}}", relativePath);

        builder.SetAppLogoOverride(iconUri)
               .AddText(service.Name)
               .AddText(message)
               .SetInlineImage(imageUri);
        AppNotificationManager.Default.Show(builder.BuildNotification());
    }

    static readonly string _applicationData;
}
