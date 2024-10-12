using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace OutputBrowser.Pages
{
    /// <summary>
    /// A output page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class OutputPage : Page
    {
        public ObservableCollection<OutputViewModel> Outputs { get; } = [];

        [ObservableProperty] string _imagePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public OutputPage() {
            _watcher.Created += WatcherEvent;
            _watcher.Changed += WatcherEvent;
            _watcher.Deleted += WatcherEvent;
            _watcher.Renamed += WatcherEvent;
            InitializeComponent();
            DataContext = this;

            Loaded += PageLoaded;
            void PageLoaded(object sender, RoutedEventArgs e) {
                Loaded -= PageLoaded;
                DispatcherQueue.TryEnqueue(() => _Outputs.Focus(FocusState.Programmatic));
            }
        }

        [RelayCommand]
        static async Task OpenFolderAsync(OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            var imageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(output.ImagePath));
            var folderLauncherOptions = new FolderLauncherOptions();
            folderLauncherOptions.ItemsToSelect.Add(imageFile);
            await Launcher.LaunchFolderAsync(imageFolder, folderLauncherOptions);
        }

        [RelayCommand]
        static async Task CopyAsync(OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems([imageFile]);
            Clipboard.SetContent(dataPackage);
        }

        [RelayCommand]
        static async Task CopyImageAsync(OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            var stream = await imageFile.OpenReadAsync();
            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            Clipboard.SetContent(dataPackage);
        }

        [RelayCommand]
        static void CopyPath(OutputViewModel output) {
            var dataPackage = new DataPackage();
            dataPackage.SetText(output.ImagePath);
            Clipboard.SetContent(dataPackage);
        }

        [RelayCommand]
        static void CopyPrompt(OutputViewModel output) {
            var dataPackage = new DataPackage();
            dataPackage.SetText(output.ContactInfo);
            Clipboard.SetContent(dataPackage);
        }

        partial void OnImagePathChanged(string value) {
            if (!string.IsNullOrWhiteSpace(value) && Directory.Exists(value)) {
                _watcher.Path = value;
                _watcher.EnableRaisingEvents = true;
                _watcher.IncludeSubdirectories = true;
            }
        }

        void WatcherEvent(object sender, FileSystemEventArgs e) {
            var fullPath = e.FullPath;
            if (Path.GetExtension(fullPath) == ".png") {
                DispatcherQueue.TryEnqueue(async () => await AddOutput(fullPath));
            }
        }

        async Task AddOutput(string path) {
            var output = new OutputViewModel(ImagePath, path);
            Outputs.Add(output);
            await output.InitializeAsync();
        }

        readonly FileSystemWatcher _watcher = new() {
            NotifyFilter = NotifyFilters.LastWrite
                           | NotifyFilters.FileName
                           | NotifyFilters.DirectoryName
        };
    }
}
