using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OutputBrowser.ViewModels;

public partial class WatchesSettingViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string Name { get; set; }

    public ObservableCollection<WatchSettingsViewModel> Watches { get; } = [];

    public WatchesSettingViewModel(SettingViewModel setting, Models.WatchesSettings watch) {
        _setting = setting;
        _watch = watch;
        Name = _watch.Name;
        _watch.Watches.ForEach(w => Watches.Add(new WatchSettingsViewModel {
            Name = w.Name,
            Path = w.Path,
            Filters = w.Filters
        }));
    }

    public void AddWatchSettings(WatchSettingsViewModel watchSettingsViewModel) {
        Watches.Add(watchSettingsViewModel);
    }

    public void Add() {
        Update();
        _setting.AddWatchesSetting(this);
    }

    public void Remove() {
        _setting.RemoveWatchesSetting(this);
    }

    public void Update() {
        _watch.Name = Name;
        _watch.Watches.Clear();
        foreach (var watch in Watches) {
            _watch.Watches.Add(new Models.WatchSettings {
                Name = watch.Name,
                Path = watch.Path,
                Filters = watch.Filters
            });
        }
    }

    readonly SettingViewModel _setting;
    readonly Models.WatchesSettings _watch;
}
