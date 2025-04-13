using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using OutputBrowser.ViewModels;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OutputBrowser.Dialogs;

public sealed partial class WatchSettingsDialog : ContentDialog
{
    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(
            nameof(Model),
            typeof(WatchSettingsViewModel),
            typeof(WatchSettingsDialog),
            new PropertyMetadata(null)
        );

    public WatchSettingsViewModel Model {
        get => (WatchSettingsViewModel)GetValue(ModelProperty);
        set {
            SetValue(ModelProperty, value);
            var isNewWatch = value.Parent == null;
            if (isNewWatch) {
                Title = _resourceLoader.GetString("WatchSettingsDialog/AddTitle");
                PrimaryButtonText = _resourceLoader.GetString("WatchSettingsDialog/AddButton");
                CloseButtonText = _resourceLoader.GetString("WatchSettingsDialog/CancelButton");
            } else {
                Title = _resourceLoader.GetString("WatchSettingsDialog/UpdateTitle");
                PrimaryButtonText = _resourceLoader.GetString("WatchSettingsDialog/UpdateButton");
                CloseButtonText = _resourceLoader.GetString("WatchSettingsDialog/CancelButton");
            }
        }
    }

    public WatchSettingsDialog() {
        InitializeComponent();
        DataContext = this;
    }

    [RelayCommand]
    async Task SelectImage() {
        var picker = new FileOpenPicker {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
        var file = await picker.PickSingleFileAsync();
        if (file != null) {
            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 120, ThumbnailOptions.ResizeThumbnail);
            using var stream = thumbnail.AsStreamForRead();
            var buffer = new byte[stream.Length];
            stream.ReadExactly(buffer, 0, buffer.Length);
            Model.Icon = buffer;
        }
    }

    [RelayCommand]
    void DeleteImage() {
        Model.Icon = null;
    }

    [RelayCommand]
    async Task SelectWatchPath() {
        var picker = new FolderPicker {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.ComputerFolder
        };
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
        var folder = await picker.PickSingleFolderAsync();
        if (folder != null) {
            Model.Path = folder.Path;
        }
    }

    static readonly ResourceLoader _resourceLoader = new();
}
