using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace OutputBrowser.ViewModels;

public partial class WatchSettingsViewModel : ObservableRecipient
{
    public WatchesSettingViewModel Parent => _parent;
    public WriteableBitmap IconSource => _icon ??= CreateImage(Icon);
    public string IconFileName => $"{Parent.Name}.{Name}.png";

    [ObservableProperty] public partial byte[] Icon { get; set; }
    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial string Path { get; set; }
    [ObservableProperty] public partial string Filters { get; set; }
    [ObservableProperty] public partial bool Notification { get; set; }

    public WatchSettingsViewModel(WatchesSettingViewModel parent) {
        _parent = parent;
    }

    public void AddTo(WatchesSettingViewModel parent) {
        _parent = parent;
        parent.AddWatchSettings(this);
    }

    partial void OnIconChanged(byte[] value) {
        _icon = null;
        OnPropertyChanged(nameof(IconSource));
    }

    static WriteableBitmap CreateImage(byte[] bytes) {
        if (bytes == null) return null;
        var bitmap = new WriteableBitmap(40, 40);
        bitmap.SetSource(new MemoryStream(bytes, 0, bytes.Length).AsRandomAccessStream());
        return bitmap;
    }

    WatchesSettingViewModel _parent;
    WriteableBitmap _icon;
}
