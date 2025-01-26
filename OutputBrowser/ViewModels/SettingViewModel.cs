using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;

namespace OutputBrowser.ViewModels;

public partial class SettingViewModel : ObservableRecipient
{
    public Models.OutputBrowserSettings Settings => _settings;

    [ObservableProperty]
    public partial string Path { get; set; }

    [ObservableProperty]
    public partial string Filters { get; set; }

    public SettingViewModel(IOptions<Models.OutputBrowserSettings> settings) {
        _settings = settings.Value;
        Path = _settings.Default.Path;
        Filters = _settings.Default.Filters;
    }

    partial void OnPathChanged(string value) {
        _settings.Default.Path = value;
    }

    partial void OnFiltersChanged(string value) {
        _settings.Default.Filters = value;
    }

    readonly Models.OutputBrowserSettings _settings;
}
