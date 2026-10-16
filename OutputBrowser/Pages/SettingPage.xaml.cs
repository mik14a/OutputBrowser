using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using OutputBrowser.Controls;
using OutputBrowser.Models;
using OutputBrowser.ViewModels;
using Windows.Storage;
using Windows.System;

namespace OutputBrowser.Pages;

/// <summary>
/// A setting page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingPage : Page
{
    public static readonly KeyValuePair<ElementTheme, string>[] Themes;
    public static readonly KeyValuePair<SystemBackdrop, string>[] Backdrops;
    public static readonly KeyValuePair<ViewerType, string>[] Viewers;

    static SettingPage() {
        Themes = Enum.GetValues<ElementTheme>()
            .Select(theme => new KeyValuePair<ElementTheme, string>(theme, _resourceLoader.GetString($"ElementTheme/{theme}")))
            .ToArray();
        Backdrops = Enum.GetValues<SystemBackdrop>()
            .Select(backdrop => new KeyValuePair<SystemBackdrop, string>(backdrop, _resourceLoader.GetString($"SystemBackdrop/{backdrop}")))
            .ToArray();
        Viewers = Enum.GetValues<ViewerType>()
            .Select(viewer => new KeyValuePair<ViewerType, string>(viewer, _resourceLoader.GetString($"ViewerType/{viewer}")))
            .ToArray();
    }

    public SettingViewModel Model => _model;

    public SettingPage() {
        _model = App.GetService<SettingViewModel>();
        InitializeComponent();
        DataContext = this;
        _Themes.SelectedItem = Themes.FirstOrDefault(t => t.Key == Model.Theme);
        _Backdrops.ItemsSource = Backdrops;
        _Backdrops.SelectedIndex = Array.FindIndex(Backdrops, pair => pair.Key == Model.Backdrop);
        _Viewers.SelectedItem = Viewers.FirstOrDefault(v => v.Key == Model.Viewer);
        _model.PropertyChanged += OnModelPropertyChanged;
    }

    [RelayCommand]
    static async Task OpenConfigFolderAsync() {
        var documentFolder = await StorageFolder.GetFolderFromPathAsync(App.PersonalDirectory);
        await Launcher.LaunchFolderAsync(documentFolder);
    }

    [RelayCommand]
    async Task SaveAsync() {
        _model.Update();
        await App.Current.SaveSettingsAsync();
        App.Current.Shell.ReturnToPageBeforeSettings();
    }

    [RelayCommand]
    void NewWatches() {
        var watches = Model.CreateWatchesSetting();
        App.Current.Shell.Navigate(typeof(WatchSettingPage), (true, watches));
    }

    [RelayCommand]
    void SelectWatches(WatchesSettingsViewModel watchesSettingViewModel) {
        App.Current.Shell.Navigate(typeof(WatchSettingPage), (false, watchesSettingViewModel));
    }

    void OnBackdropSelectionChanged(object sender, SelectionChangedEventArgs e) {
        var index = _Backdrops.SelectedIndex;
        if (0 > index) return;
        var backdrop = Backdrops[index].Key;
        if (backdrop == Model.Backdrop) return;
        Model.Backdrop = backdrop;
    }

    void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(SettingViewModel.Theme)) {
            App.Current.SetElementTheme(Model.Theme);
        } else if (e.PropertyName == nameof(SettingViewModel.Backdrop)) {
            var backdrop = Model.Backdrop;
            DispatcherQueue.TryEnqueue(() => App.Current.SetSystemBackdrop(backdrop));
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => {
                if (!IsLoaded) return;
                var index = Array.FindIndex(Backdrops, pair => pair.Key == backdrop);
                _Backdrops.ItemsSource = null;
                _Backdrops.ItemsSource = Backdrops;
                _Backdrops.SelectedIndex = index;
            });
        }
    }

    readonly SettingViewModel _model;
    static readonly ResourceLoader _resourceLoader = new();
}
