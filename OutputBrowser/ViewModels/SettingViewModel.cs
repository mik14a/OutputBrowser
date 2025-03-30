using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;

namespace OutputBrowser.ViewModels;

public partial class SettingViewModel : ObservableRecipient
{
    public WatchSettingsViewModel Default { get; set; } = new();

    public ObservableCollection<WatchesSettingViewModel> Watches { get; } = [];

    public SettingViewModel(IOptions<Models.OutputBrowserSettings> settings) {
        _settings = settings.Value;
        Apply();
    }

    public void Apply() {
        Default.Path = _settings.Default.Path;
        Default.Filters = _settings.Default.Filters;
        foreach (var watch in _settings.Watches) {
            Watches.Add(new WatchesSettingViewModel(this, watch.Value));
        }
    }

    public void Update() {
        _settings.Default.Path = Default.Path;
        _settings.Default.Filters = Default.Filters;
        _settings.Watches.Clear();
        foreach (var watch in Watches) {
            _settings.Watches.Add(watch.Name, new Models.WatchesSettings {
                Name = watch.Name,
                Watches = [.. watch.Watches.Select(w => new Models.WatchSettings {
                    Icon = w.Icon,
                    Name = w.Name,
                    Path = w.Path,
                    Filters = w.Filters
                })]
            });
        }
    }

    public WatchesSettingViewModel CreateWatchesSetting() {
        var watch = new Models.WatchesSettings() {
            Name = "新規監視"
        };
        return new WatchesSettingViewModel(this, watch);
    }

    public void AddWatchesSetting(WatchesSettingViewModel watchesSettingViewModel) {
        Watches.Add(watchesSettingViewModel);
    }

    public void RemoveWatchesSetting(WatchesSettingViewModel watchesSettingViewModel) {
        Watches.Remove(watchesSettingViewModel);
    }

    readonly Models.OutputBrowserSettings _settings;
}
