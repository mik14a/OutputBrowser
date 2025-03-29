using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OutputBrowser.ViewModels;

namespace OutputBrowser.Pages
{
    /// <summary>
    /// A WatchSetting page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WatchSettingPage : Page
    {
        public bool IsNewWatchesSetting { get; set; }

        public WatchesSettingViewModel Model { get; private set; }

        public WatchSettingPage() {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var parameter = e.Parameter;
            (IsNewWatchesSetting, Model) = (ValueTuple<bool, WatchesSettingViewModel>)parameter;
            base.OnNavigatedTo(e);
        }

        [RelayCommand]
        async Task AddWatchSettingsAsync() {
            var watchSettingsViewModel = new WatchSettingsViewModel();
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
            Model.AddWatchSettings(watchSettingsViewModel);
            _WatchSettingsDialog.Hide();
        }

        [RelayCommand]
        async Task OpenWatchSettingsAsync(WatchSettingsViewModel watchSettingsViewModel) {
            var newWatchSettingsViewModel = new WatchSettingsViewModel {
                Name = watchSettingsViewModel.Name,
                Path = watchSettingsViewModel.Path,
                Filters = watchSettingsViewModel.Filters
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
            watchSettingsViewModel.Name = newWatchSettingsViewModel.Name;
            watchSettingsViewModel.Path = newWatchSettingsViewModel.Path;
            watchSettingsViewModel.Filters = newWatchSettingsViewModel.Filters;
            _WatchSettingsDialog.Hide();
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
