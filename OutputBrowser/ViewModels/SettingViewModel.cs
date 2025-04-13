using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;

namespace OutputBrowser.ViewModels;

public partial class SettingViewModel : ObservableRecipient
{
    [ObservableProperty] public partial ElementTheme Theme { get; set; }
    [ObservableProperty] public partial Controls.SystemBackdrop Backdrop { get; set; }

    public WatchSettingsViewModel Default { get; set; } = new(null);
    public ObservableCollection<WatchesSettingsViewModel> Watches { get; } = [];

    public SettingViewModel(IOptions<Models.OutputBrowserSettings> settings) {
        _settings = settings.Value;
        Apply();
    }

    public void Apply() {
        Theme = Enum.TryParse<ElementTheme>(_settings.Theme, out var theme) ? theme : ElementTheme.Default;
        Backdrop = Enum.TryParse<Controls.SystemBackdrop>(_settings.Backdrop, out var backdrop) ? backdrop : Controls.SystemBackdrop.Mica;
        Default.Path = _settings.Default.Path;
        Default.Filters = _settings.Default.Filters;
        Default.Format = _settings.Default.Format;
        foreach (var watch in _settings.Watches) {
            Watches.Add(new WatchesSettingsViewModel(this, watch.Value));
        }
    }

    public void Update() {
        _settings.Theme = Theme.ToString();
        _settings.Backdrop = Backdrop.ToString();
        _settings.Default.Path = Default.Path;
        _settings.Default.Filters = Default.Filters;
        _settings.Default.Format = Default.Format;
        _settings.Watches.Clear();
        foreach (var watch in Watches) {
            _settings.Watches.Add(watch.Name, new Models.WatchesSettings {
                Icon = watch.Icon.ToString(),
                Name = watch.Name,
                Watches = [.. watch.Watches.Select(w => new Models.WatchSettings {
                    Icon = w.Icon != null && 0 < w.Icon.Length ? w.Icon : null,
                    Name = w.Name,
                    Path = w.Path,
                    Filters = w.Filters,
                    Format = w.Format,
                    Notification = w.Notification
                })]
            });
        }
    }

    public WatchesSettingsViewModel CreateWatchesSetting() {
        var watch = new Models.WatchesSettings() {
            Name = "新規監視"
        };
        return new WatchesSettingsViewModel(this, watch);
    }

    public void AddWatchesSetting(WatchesSettingsViewModel watchesSettingViewModel) {
        Watches.Add(watchesSettingViewModel);
    }

    public void RemoveWatchesSetting(WatchesSettingsViewModel watchesSettingViewModel) {
        Watches.Remove(watchesSettingViewModel);
    }

    readonly Models.OutputBrowserSettings _settings;
}
