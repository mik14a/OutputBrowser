using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.VisualBasic;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace OutputBrowser.ViewModels
{
    public partial class OutputViewModel : ObservableRecipient
    {
        public string ImagePath { get; }
        public string FileName { get; }
        public string DisplayName { get; }
        public DateTime DateModified { get; }
        public long Size { get; }

        [ObservableProperty] bool _visibleContactInfo = false;
        [ObservableProperty] ImageSource _image;
        [ObservableProperty] string _contactInfo;

        public OutputViewModel(string basePath, string fullPath) {
            ImagePath = fullPath;
            FileName = Path.GetFileName(fullPath);
            DisplayName = Path.GetRelativePath(basePath, fullPath);
            var fileInfo = new FileInfo(fullPath);
            DateModified = fileInfo.LastWriteTime;
            Size = fileInfo.Length;
        }

        public async Task InitializeAsync() {
            var file = await StorageFile.GetFileFromPathAsync(ImagePath);
            using var stream = await file.OpenAsync(FileAccessMode.Read);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            Image = bitmap;
            var decoder = await BitmapDecoder.CreateAsync(stream);
            const string Parameters = "/tEXt/{str=parameters}";
            var retrieveProperties = await decoder.BitmapProperties.GetPropertiesAsync([Parameters]);
            retrieveProperties.TryGetValue(Parameters, out var parameters);
            ContactInfo = (string)parameters?.Value;
        }
    }
}
