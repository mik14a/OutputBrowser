using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.Extensions;
using OutputBrowser.Models;
using OutputBrowser.Services;
using OutputBrowser.ViewModels;
using Windows.System;

namespace OutputBrowser.Pages
{
    /// <summary>
    /// A output page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class OutputPage : Page, IDisposable
    {
        [ObservableProperty]
        string _path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        [ObservableProperty]
        string _filters = "*.png";

        public ObservableCollection<OutputViewModel> Outputs { get; } = [];
        [ObservableProperty]
        bool _isScrolledAway;

        public OutputPage() {
            var setting = File.Exists(App.SettingsFile)
               ? JsonSerializer.Deserialize<OutputBrowserSettings>(File.ReadAllText(App.SettingsFile))?.Default
               : new WatchSettings { Path = _path, Filters = _filters };

            _path = setting.Path;
            _filters = setting.Filters;
            _service = new FileSystemWatchService(setting) {
                Filters = _filters
            };
            _service.Changed += OnChanged;
            _service.Deleted += OnDeleted;
            _service.Renamed += OnRenamed;
            Loaded += PageLoaded;
            Unloaded += PageUnloaded;

            InitializeComponent();
            DataContext = this;

            _Outputs.Loaded += (sender, args)
                => GetScrollViewer((ListView)sender).ViewChanged += (sender, e)
                    => IsScrolledAway = ((ScrollViewer)sender).ScrollableHeight != ((ScrollViewer)sender).VerticalOffset;

            void PageLoaded(object sender, RoutedEventArgs e) {
                Loaded -= PageLoaded;
                _Outputs.Focus(FocusState.Programmatic);
            }
            void PageUnloaded(object sender, RoutedEventArgs e) {
                Unloaded -= PageUnloaded;
                _service.Changed -= OnChanged;
                _service.Deleted -= OnDeleted;
                _service.Renamed -= OnRenamed;
                var settings = new OutputBrowserSettings { Default = new WatchSettings { Path = _path, Filters = _filters } };
                File.WriteAllText(App.SettingsFile, JsonSerializer.Serialize(settings));
            }
        }

        [RelayCommand]
        static async Task OpenWithDefaultAppAsync(OutputViewModel output) {
            var imageFile = await StorageFile.GetFileFromPathAsync(output.ImagePath);
            await Launcher.LaunchFileAsync(imageFile);
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

        partial void OnPathChanged(string value) {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (!Directory.Exists(value)) return;
            if (_service.Path == value) return;
            _service.Path = value;
        }

        partial void OnFiltersChanged(string value) {
            if (string.IsNullOrWhiteSpace(value)) return;
            _service.Filters = value;
        }

        void OnChanged(object sender, FileSystemEventArgs e) {
            DispatcherQueue.TryEnqueue(async () => {
                if (Outputs.Any(o => o.ImagePath == e.FullPath)) return;
                var output = new OutputViewModel(Path, e.FullPath);
                Outputs.Add(output);
                await output.InitializeAsync();
            });
        }

        void OnDeleted(object sender, FileSystemEventArgs e) {
            DispatcherQueue.TryEnqueue(() => {
                var viewModel = Outputs.FirstOrDefault(o => o.ImagePath == e.FullPath);
                if (viewModel != null) Outputs.Remove(viewModel);
            });
        }

        void OnRenamed(object sender, RenamedEventArgs e) {
            DispatcherQueue.TryEnqueue(async () => {
                var viewModel = Outputs.FirstOrDefault(o => o.ImagePath == e.OldFullPath);
                if (viewModel != null) Outputs.Remove(viewModel);
                if (Outputs.Any(o => o.ImagePath == e.FullPath)) return;
                var output = new OutputViewModel(Path, e.FullPath);
                Outputs.Add(output);
                await output.InitializeAsync();
            });
        }

        readonly FileSystemWatchService _service;

        public void Dispose() {
            ((IDisposable)_service).Dispose();
        }
    }
}
