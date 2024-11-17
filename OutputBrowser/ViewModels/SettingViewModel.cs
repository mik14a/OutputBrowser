using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;

namespace OutputBrowser.ViewModels;

public partial class SettingViewModel : ObservableRecipient
{
    public Models.OutputBrowserSettings Settings => _settings;

    [ObservableProperty] string _path;
    [ObservableProperty] string _filters;

    public SettingViewModel(IOptions<Models.OutputBrowserSettings> settings) {
        _settings = settings.Value;
        _path = _settings.Default.Path;
        _filters = _settings.Default.Filters;
    }

    partial void OnPathChanged(string value) {
        _settings.Default.Path = value;
    }

    partial void OnFiltersChanged(string value) {
        _settings.Default.Filters = value;
    }

    readonly Models.OutputBrowserSettings _settings;
}
