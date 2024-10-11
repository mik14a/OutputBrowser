using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using OutputBrowser.ViewModels;

namespace OutputBrowser.Pages
{
    /// <summary>
    /// A output page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class OutputPage : Page
    {
        public ObservableCollection<OutputViewModel> Outputs { get; } = [];

        [ObservableProperty] string _path = null;

        public OutputPage() {
            _watcher.Created += WatcherEvent;
            _watcher.Changed += WatcherEvent;
            _watcher.Deleted += WatcherEvent;
            _watcher.Renamed += WatcherEvent;
            InitializeComponent();
            DataContext = this;
        }

        partial void OnPathChanged(string value) {
            if (!string.IsNullOrWhiteSpace(value) && System.IO.Directory.Exists(value)) {
                _watcher.Path = value;
                _watcher.EnableRaisingEvents = true;
                _watcher.IncludeSubdirectories = true;
            }
        }

        void WatcherEvent(object sender, FileSystemEventArgs e) {
            var fullPath = e.FullPath;
            if (System.IO.Path.GetExtension(fullPath) == ".png") {
                DispatcherQueue.TryEnqueue(() => Outputs.Add(new OutputViewModel(fullPath)));
            }
        }

        readonly FileSystemWatcher _watcher = new() {
            NotifyFilter = NotifyFilters.LastWrite
                           | NotifyFilters.FileName
                           | NotifyFilters.DirectoryName
        };
    }
}
