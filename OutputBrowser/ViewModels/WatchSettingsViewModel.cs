using CommunityToolkit.Mvvm.ComponentModel;

namespace OutputBrowser.ViewModels;

public partial class WatchSettingsViewModel : ObservableRecipient
{
    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial string Path { get; set; }
    [ObservableProperty] public partial string Filters { get; set; }
}
