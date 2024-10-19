using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
    public sealed partial class OutputPage : Page, IDisposable
    {
        [ObservableProperty] string _imagePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public ObservableCollection<OutputViewModel> Outputs { get; } = [];
        [ObservableProperty] bool _isScrolledAway;

        public OutputPage() {
            if (File.Exists(App.SettingsFile)) {
                var imagePath = File.ReadAllText(App.SettingsFile);
                if (Directory.Exists(imagePath)) {
                    _imagePath = imagePath;
                }
            }
            _watcher = new(_imagePath) {
                NotifyFilter = NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };
            _watcher.Created += WatcherEvent;
            _watcher.Changed += WatcherEvent;
            _watcher.Deleted += WatcherEvent;
            _watcher.Renamed += WatcherEvent;
            Loaded += PageLoaded;
            Unloaded += PageUnloaded;

            InitializeComponent();
            DataContext = this;

            _Outputs.Loaded += (sender, args)
                => GetScrollViewer((ListView)sender).ViewChanged += (sender, e)
                    => IsScrolledAway = ((ScrollViewer)sender).ScrollableHeight != ((ScrollViewer)sender).VerticalOffset;

            void PageLoaded(object sender, RoutedEventArgs e) {
                Loaded -= PageLoaded;
                _ImagePath.Focus(FocusState.Programmatic);
            }
            void PageUnloaded(object sender, RoutedEventArgs e) {
                Unloaded -= PageUnloaded;
                File.WriteAllText(App.SettingsFile, _imagePath);
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

        [RelayCommand]
        void ScrollToBottom() {
            ScrollToBottom(_Outputs, false);
        }

        partial void OnImagePathChanged(string value) {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (!Directory.Exists(value)) return;
            if (_watcher.Path == value) return;
            _watcher.Path = value;
        }

        void WatcherEvent(object sender, FileSystemEventArgs e) {
            if (!File.Exists(e.FullPath)) return;
            if (!_extensions.Contains(Path.GetExtension(e.FullPath))) return;
            DispatcherQueue.TryEnqueue(async () => await AddOutput(e.FullPath));
        }

        async Task AddOutput(string path) {
            var output = new OutputViewModel(ImagePath, path);
            Outputs.Add(output);
            await output.InitializeAsync();
        }

        readonly FileSystemWatcher _watcher;

        public void Dispose() {
            ((IDisposable)_watcher).Dispose();
        }

        public static ScrollViewer GetScrollViewer(ListView listView) {
            var child = VisualTreeHelper.GetChild(listView, 0);
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++) {
                var obj = VisualTreeHelper.GetChild(child, i);
                if (obj is not ScrollViewer scrollViewer) continue;
                return scrollViewer;
            }
            return null;
        }

        public static void ScrollToBottom(ListView listView, bool disableAnimation) {
            var scrollViewer = GetScrollViewer(listView);
            scrollViewer.ChangeView(0.0f, scrollViewer.ExtentHeight, 1.0f, disableAnimation);
        }

        static readonly List<string> _extensions = [".png"];
    }
}
