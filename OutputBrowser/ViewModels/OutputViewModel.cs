using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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
            try {
                DateModified = fileInfo.LastWriteTime;
                Size = fileInfo.Length;
            } catch (FileNotFoundException ex) {
                Debug.WriteLine(ex.Message);  // File was deleted
            }
        }

        public async Task<bool> InitializeAsync() {

            if (!File.Exists(ImagePath)) return false;  // File not found
            if (Directory.Exists(ImagePath)) return false;  // Is directory

            var canOpenRead = false;
            const int MaxRetry = 10;
            const int Delay = 100;
            for (var retry = 0; retry < MaxRetry; retry++) {
                try {
                    // Check if file is still being written by another process
                    using var fileStream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read);
                    canOpenRead = true;
                    break;
                } catch (IOException ex) {
                    Debug.WriteLine(ex.Message);  // Wait for image writing to complete
                    await Task.Delay(Delay);
                }
            }
            if (!canOpenRead) return false;

            try {
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
                return true;
            } catch (FileNotFoundException ex) {
                Debug.WriteLine(ex.Message);  // File deleted in operating
            } catch (UnauthorizedAccessException ex) {
                Debug.WriteLine(ex.Message);  // Can not access
            } catch (COMException ex) {
                Debug.WriteLine(ex.Message);  // Not a image file
            }
            return false;
        }
    }
}
