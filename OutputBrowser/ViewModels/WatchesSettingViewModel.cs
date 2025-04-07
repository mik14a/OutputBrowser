using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace OutputBrowser.ViewModels;

public partial class WatchesSettingViewModel : ObservableRecipient
{
    public Page Page { get; set; }

    [ObservableProperty]
    public partial Symbol Icon { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    public ObservableCollection<WatchSettingsViewModel> Watches { get; } = [];

    public WatchesSettingViewModel(SettingViewModel setting, Models.WatchesSettings watch) {
        _setting = setting;
        _watch = watch;
        Icon = Enum.TryParse<Symbol>(watch.Icon, out var icon) ? icon : Symbol.Link;
        Name = _watch.Name;
        _watch.Watches.ForEach(w => Watches.Add(new WatchSettingsViewModel(this) {
            Icon = w.Icon,
            Name = w.Name,
            Path = w.Path,
            Filters = w.Filters,
            Notification = w.Notification
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
        _watch.Icon = Icon.ToString();
        _watch.Name = Name;
        _watch.Watches.Clear();
        foreach (var watch in Watches) {
            _watch.Watches.Add(new Models.WatchSettings {
                Icon = watch.Icon,
                Name = watch.Name,
                Path = watch.Path,
                Filters = watch.Filters,
                Notification = watch.Notification
            });
        }
    }

    readonly SettingViewModel _setting;
    readonly Models.WatchesSettings _watch;
}
