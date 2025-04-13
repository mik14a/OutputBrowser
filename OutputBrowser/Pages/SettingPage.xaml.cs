using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.Controls;
using OutputBrowser.ViewModels;
using Windows.Storage;
using Windows.System;

namespace OutputBrowser.Pages;

/// <summary>
/// A setting page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingPage : Page
{
    public static readonly KeyValuePair<ElementTheme, string>[] Themes = [
        new(ElementTheme.Default, "システム"),
        new(ElementTheme.Light, "ライト"),
        new(ElementTheme.Dark, "ダーク")
    ];

    public static readonly KeyValuePair<SystemBackdrop, string>[] Backdrops = [
        new(SystemBackdrop.Mica, "マイカ"),
        new(SystemBackdrop.MicaAlt, "マイカオルタナティブ"),
        new(SystemBackdrop.Acrylic, "アクリル"),
    ];

    public SettingViewModel Model => _model;

    public SettingPage() {
        _model = App.GetService<SettingViewModel>();
        InitializeComponent();
        DataContext = this;
        _Themes.SelectedItem = Themes.FirstOrDefault(t => t.Key == Model.Theme);
        _Backdrops.SelectedItem = Backdrops.FirstOrDefault(b => b.Key == Model.Backdrop);
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

    void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(SettingViewModel.Theme)) {
            App.Current.SetElementTheme(Model.Theme);
        } else if (e.PropertyName == nameof(SettingViewModel.Backdrop)) {
            App.Current.SetSystemBackdrop(Model.Backdrop);
        }
    }

    readonly SettingViewModel _model;
}
