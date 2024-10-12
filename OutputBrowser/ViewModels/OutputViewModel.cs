using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace OutputBrowser.ViewModels
{
    public partial class OutputViewModel : ObservableRecipient
    {
        [ObservableProperty] string _imagePath = null;
        [ObservableProperty] bool _visibleContactInfo = false;
        public ImageSource Image => _image ??= CreateImage();
        public string ContactInfo => _contactInfo ?? GetContactInfo();

        public OutputViewModel(string path) {
            _imagePath = path;
        }

        BitmapSource CreateImage() {
            return !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath)
                ? CreateImage(ImagePath)
                : null;

            static BitmapImage CreateImage(string path) {
                var bitmap = new BitmapImage();
                var file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
                using var stream = file.OpenAsync(FileAccessMode.Read).GetAwaiter().GetResult();
                bitmap.SetSource(stream);
                return bitmap;
            }
        }

        string GetContactInfo() {
            return !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath)
                ? GetContactInfo(ImagePath)
                : null;

            static string GetContactInfo(string path) {
                var file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
                using var stream = file.OpenAsync(FileAccessMode.Read).GetAwaiter().GetResult();
                var decoder = BitmapDecoder.CreateAsync(stream).GetAwaiter().GetResult();
                var retrieveProperties = decoder.BitmapProperties.GetPropertiesAsync(["/tEXt/{str=parameters}"]).GetAwaiter().GetResult();
                return retrieveProperties.TryGetValue("/tEXt/{str=parameters}", out var parameters)
                    ? (string)parameters.Value
                    : null;
            }
        }

        ImageSource _image = null;
        readonly string _contactInfo = null;
    }
}
