using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace OutputBrowser.ViewModels
{
    public partial class OutputViewModel : ObservableRecipient
    {
        [ObservableProperty] string _imagePath;
        public ImageSource Image => _image ??= CreateImage();

        public OutputViewModel(string path) {
            _imagePath = path;
        }

        BitmapSource CreateImage() {
            return !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath)
                ? CreateBitmapImage(ImagePath)
                : null;

            static BitmapImage CreateBitmapImage(string path) {
                var bitmap = new BitmapImage();
                var file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
                var stream = file.OpenReadAsync().GetAwaiter().GetResult();
                bitmap.SetSource(stream);
                return bitmap;
            }
        }

        ImageSource _image = null;
    }
}
