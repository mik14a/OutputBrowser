using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OutputBrowser.ViewModels;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OutputBrowser.Pages
{
    /// <summary>
    /// A WatchSetting page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WatchSettingPage : Page
    {
        public record struct SymbolIcon(string Name, Symbol Icon);
        public static SymbolIcon[] Icons { get; } = [.. Enum.GetValues<Symbol>().Select(s => new SymbolIcon(Enum.GetName(s), s))];

        public bool IsNewWatchesSetting { get; set; }

        public WatchesSettingViewModel Model { get; private set; }

        public WatchSettingPage() {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var parameter = e.Parameter;
            (IsNewWatchesSetting, Model) = (ValueTuple<bool, WatchesSettingViewModel>)parameter;
            _Icon.SelectedItem = Icons.FirstOrDefault(i => i.Icon == Model.Icon);
            base.OnNavigatedTo(e);
        }

        [RelayCommand]
        async Task AddWatchSettingsAsync() {
            var watchSettingsViewModel = new WatchSettingsViewModel(null);
            watchSettingsViewModel.PropertyChanged += OnWatchSettingPropertyChanged;
            _WatchSettingsDialog.Title = "監視設定追加";
            _WatchSettingsDialog.DataContext = watchSettingsViewModel;
            _WatchSettingsDialog.PrimaryButtonText = "追加";
            _WatchSettingsDialog.CloseButtonText = "キャンセル";
            _WatchSettingsDialog.PrimaryButtonCommand = AddWatchSettingsAddCommand;
            _WatchSettingsDialog.PrimaryButtonCommandParameter = watchSettingsViewModel;
            _WatchSettingsDialog.IsPrimaryButtonEnabled = false;
            await _WatchSettingsDialog.ShowAsync();
            watchSettingsViewModel.PropertyChanged -= OnWatchSettingPropertyChanged;
        }

        [RelayCommand]
        void AddWatchSettingsAdd(WatchSettingsViewModel watchSettingsViewModel) {
            watchSettingsViewModel.AddTo(Model);
            _WatchSettingsDialog.Hide();
        }

        [RelayCommand]
        async Task OpenWatchSettingsAsync(WatchSettingsViewModel watchSettingsViewModel) {
            var newWatchSettingsViewModel = new WatchSettingsViewModel(watchSettingsViewModel.Parent) {
                Icon = watchSettingsViewModel.Icon,
                Name = watchSettingsViewModel.Name,
                Path = watchSettingsViewModel.Path,
                Filters = watchSettingsViewModel.Filters,
                Notification = watchSettingsViewModel.Notification
            };
            newWatchSettingsViewModel.PropertyChanged += OnWatchSettingPropertyChanged;
            _WatchSettingsDialog.Title = "監視設定更新";
            _WatchSettingsDialog.DataContext = newWatchSettingsViewModel;
            _WatchSettingsDialog.PrimaryButtonText = "更新";
            _WatchSettingsDialog.CloseButtonText = "キャンセル";
            _WatchSettingsDialog.PrimaryButtonCommand = OpenWatchSettingsUpdateCommand;
            _WatchSettingsDialog.PrimaryButtonCommandParameter = (watchSettingsViewModel, newWatchSettingsViewModel);
            _WatchSettingsDialog.IsPrimaryButtonEnabled = true;
            await _WatchSettingsDialog.ShowAsync();
            newWatchSettingsViewModel.PropertyChanged -= OnWatchSettingPropertyChanged;
        }

        [RelayCommand]
        void OpenWatchSettingsUpdate(ValueTuple<WatchSettingsViewModel, WatchSettingsViewModel> parameter) {
            var (watchSettingsViewModel, newWatchSettingsViewModel) = parameter;
            watchSettingsViewModel.Icon = newWatchSettingsViewModel.Icon;
            watchSettingsViewModel.Name = newWatchSettingsViewModel.Name;
            watchSettingsViewModel.Path = newWatchSettingsViewModel.Path;
            watchSettingsViewModel.Filters = newWatchSettingsViewModel.Filters;
            watchSettingsViewModel.Notification = newWatchSettingsViewModel.Notification;
            _WatchSettingsDialog.Hide();
        }

        [RelayCommand]
        async Task SelectImage(WatchSettingsViewModel watchSettingsViewModel) {
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
                watchSettingsViewModel.Icon = buffer;
            }
        }

        [RelayCommand]
        void Update() {
            if (IsNewWatchesSetting) {
                Model.Add();
            } else {
                Model.Update();
            }
            App.Current.Shell.GoBack();
        }

        [RelayCommand]
        void Delete() {
            Model.Remove();
            App.Current.Shell.GoBack();
        }

        [RelayCommand]
        void Cancel() {
            App.Current.Shell.GoBack();
        }

        void OnWatchSettingPropertyChanged(object sender, PropertyChangedEventArgs e) {
            var vm = (WatchSettingsViewModel)sender;
            _WatchSettingsDialog.IsPrimaryButtonEnabled
                = !string.IsNullOrEmpty(vm.Name)
                && !string.IsNullOrEmpty(vm.Path)
                && !string.IsNullOrEmpty(vm.Filters);
        }
    }
}
