using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace OutputBrowser.Helpers;

static class ImageHelper
{
    public static async Task SaveToFileAsync(this WriteableBitmap image, string folderName, string fileName) {
        var path = Path.Combine(folderName, fileName);
        var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
        await image.SaveToFileAsync(folder, Path.GetFileName(path));
    }

    static async Task SaveToFileAsync(this WriteableBitmap image, StorageFolder folder, string fileName) {
        var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        using var stream = await file.OpenStreamForWriteAsync();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());
        var pixelStream = image.PixelBuffer.AsStream();
        var pixels = new byte[image.PixelBuffer.Length];
        await pixelStream.ReadExactlyAsync(pixels);
        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)image.PixelWidth, (uint)image.PixelHeight, 96, 96, pixels);
        await encoder.FlushAsync();
    }
}
