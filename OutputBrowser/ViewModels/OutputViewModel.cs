using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace OutputBrowser.ViewModels;

public partial class OutputViewModel : ObservableRecipient
{
    public string Sender { get; }
    public ImageSource Icon { get; }
    public string ImagePath { get; }
    public string FileName { get; }
    public string DisplayName { get; }
    public DateTime DateModified { get; }
    public long Size { get; }

    public bool IsDefault { get; }

    [ObservableProperty]
    public partial bool VisibleContactInfo { get; set; } = false;

    [ObservableProperty]
    public partial ImageSource Image { get; set; }

    [ObservableProperty]
    public partial string ContactInfo { get; set; }

    public OutputViewModel(string sender, ImageSource icon, string format, string basePath, string fullPath) {
        Sender = sender;
        Icon = icon;
        ImagePath = fullPath;
        FileName = Path.GetFileName(fullPath);
        var relativePath = Path.GetRelativePath(basePath, fullPath);
        DisplayName = !string.IsNullOrWhiteSpace(format)
                      ? format.Replace(Models.WatchSettings.FileName, FileName, StringComparison.OrdinalIgnoreCase)
                              .Replace(Models.WatchSettings.FilePath, relativePath, StringComparison.OrdinalIgnoreCase)
                      : relativePath;
        var fileInfo = new FileInfo(fullPath);
        try {
            DateModified = fileInfo.LastWriteTime;
            Size = fileInfo.Length;
        } catch (FileNotFoundException ex) {
            Debug.WriteLine(ex.Message);  // File was deleted
        }
        IsDefault = !sender.Equals("Default", StringComparison.OrdinalIgnoreCase);
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

    [RelayCommand]
    async Task OpenWithDefaultAppAsync() {
        var imageFile = await StorageFile.GetFileFromPathAsync(ImagePath);
        await Launcher.LaunchFileAsync(imageFile);
    }

    [RelayCommand]
    async Task OpenFolderAsync() {
        var imageFile = await StorageFile.GetFileFromPathAsync(ImagePath);
        var imageFolder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(ImagePath));
        var folderLauncherOptions = new FolderLauncherOptions();
        folderLauncherOptions.ItemsToSelect.Add(imageFile);
        await Launcher.LaunchFolderAsync(imageFolder, folderLauncherOptions);
    }

    [RelayCommand]
    async Task CopyAsync() {
        var imageFile = await StorageFile.GetFileFromPathAsync(ImagePath);
        var dataPackage = new DataPackage();
        dataPackage.SetStorageItems([imageFile]);
        Clipboard.SetContent(dataPackage);
    }

    [RelayCommand]
    async Task CopyImageAsync() {
        var imageFile = await StorageFile.GetFileFromPathAsync(ImagePath);
        var stream = await imageFile.OpenReadAsync();
        var dataPackage = new DataPackage();
        dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
        Clipboard.SetContent(dataPackage);
    }

    [RelayCommand]
    void CopyPath() {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ImagePath);
        Clipboard.SetContent(dataPackage);
    }

    [RelayCommand]
    void CopyPrompt() {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ContactInfo);
        Clipboard.SetContent(dataPackage);
    }
}
