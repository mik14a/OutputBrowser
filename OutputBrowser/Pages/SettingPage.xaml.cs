using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.ViewModels;
using Windows.Storage;
using Windows.System;

namespace OutputBrowser.Pages;

/// <summary>
/// A setting page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingPage : Page
{
    public SettingViewModel Model => _model;

    public SettingPage() {
        _model = App.GetService<SettingViewModel>();
        InitializeComponent();
        DataContext = this;
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
    void SelectWatches(WatchesSettingViewModel watchesSettingViewModel) {
        App.Current.Shell.Navigate(typeof(WatchSettingPage), (false, watchesSettingViewModel));
    }

    readonly SettingViewModel _model;
}
