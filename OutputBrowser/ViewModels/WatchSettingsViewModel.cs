using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace OutputBrowser.ViewModels;

public partial class WatchSettingsViewModel : ObservableRecipient
{
    public ImageSource IconSource => _icon ??= CreateImage(Icon);

    [ObservableProperty] public partial byte[] Icon { get; set; }
    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial string Path { get; set; }
    [ObservableProperty] public partial string Filters { get; set; }

    ImageSource _icon;

    partial void OnIconChanged(byte[] value) {
        _icon = null;
        OnPropertyChanged(nameof(IconSource));
    }

    static ImageSource CreateImage(byte[] bytes) {
        if (bytes == null) return null;
        var bitmap = new BitmapImage();
        bitmap.SetSource(new MemoryStream(bytes, 0, bytes.Length).AsRandomAccessStream());
        return bitmap;
    }
}
