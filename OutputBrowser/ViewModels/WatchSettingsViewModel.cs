using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace OutputBrowser.ViewModels;

public partial class WatchSettingsViewModel : ObservableRecipient
{
    public WatchesSettingsViewModel Parent => _parent;
    public WriteableBitmap IconSource => _icon ??= CreateImage(Icon);
    public string IconFileName => $"{Parent.Name}.{Name}.png";

    [ObservableProperty] public partial byte[] Icon { get; set; }
    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial string Path { get; set; }
    [ObservableProperty] public partial string Filters { get; set; }
    [ObservableProperty] public partial string Format { get; set; }
    [ObservableProperty] public partial bool Notification { get; set; }

    [ObservableProperty] public partial bool IsValid { get; private set; }

    public WatchSettingsViewModel(WatchesSettingsViewModel parent) {
        _parent = parent;
        PropertyChanged += WatchSettingsViewModelPropertyChanged;
    }

    public void AddTo(WatchesSettingsViewModel parent) {
        _parent = parent;
        parent.AddWatchSettings(this);
    }

    void WatchSettingsViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (sender is not WatchSettingsViewModel vm) return;
        vm.IsValid = !string.IsNullOrWhiteSpace(vm.Name)
                     && !string.IsNullOrWhiteSpace(vm.Path)
                     && !string.IsNullOrWhiteSpace(vm.Filters);
    }

    partial void OnIconChanged(byte[] value) {
        _icon = null;
        OnPropertyChanged(nameof(IconSource));
    }

    WatchesSettingsViewModel _parent;
    WriteableBitmap _icon;

    static WriteableBitmap CreateImage(byte[] bytes) {
        if (bytes == null || bytes.Length == 0) return null;
        var bitmap = new WriteableBitmap(40, 40);
        bitmap.SetSource(new MemoryStream(bytes, 0, bytes.Length).AsRandomAccessStream());
        return bitmap;
    }
}
