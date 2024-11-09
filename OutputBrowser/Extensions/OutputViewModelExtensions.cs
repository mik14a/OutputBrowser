using System;
using System.Threading.Tasks;
using OutputBrowser.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace OutputBrowser.Extensions
{
    public static class OutputViewModelExtensions
    {
        public static async Task OpenWithDefaultAppAsync(this OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            await Launcher.LaunchFileAsync(imageFile);
        }

        public static async Task OpenFolderAsync(this OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            var imageFolder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(output.ImagePath));
            var folderLauncherOptions = new FolderLauncherOptions();
            folderLauncherOptions.ItemsToSelect.Add(imageFile);
            await Launcher.LaunchFolderAsync(imageFolder, folderLauncherOptions);
        }

        public static async Task CopyAsync(this OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems([imageFile]);
            Clipboard.SetContent(dataPackage);
        }

        public static async Task CopyImageAsync(this OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            var stream = await imageFile.OpenReadAsync();
            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            Clipboard.SetContent(dataPackage);
        }

        public static void CopyPath(this OutputViewModel output) {
            var dataPackage = new DataPackage();
            dataPackage.SetText(output.ImagePath);
            Clipboard.SetContent(dataPackage);
        }

        public static void CopyPrompt(this OutputViewModel output) {
            var dataPackage = new DataPackage();
            dataPackage.SetText(output.ContactInfo);
            Clipboard.SetContent(dataPackage);
        }
    }
}
