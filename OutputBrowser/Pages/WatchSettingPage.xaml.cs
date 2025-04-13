using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using OutputBrowser.Models;
using OutputBrowser.ViewModels;

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

        public WatchesSettingsViewModel Model { get; private set; }

        public WatchSettingPage() {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var parameter = e.Parameter;
            (IsNewWatchesSetting, Model) = (ValueTuple<bool, WatchesSettingsViewModel>)parameter;
            _Icon.SelectedItem = Icons.FirstOrDefault(i => i.Icon == Model.Icon);
            base.OnNavigatedTo(e);
        }

        [RelayCommand]
        async Task AddWatchSettingsAsync() {
            var watchSettingsViewModel = new WatchSettingsViewModel(null) {
                Icon = null,
                Name = _resourceLoader.GetString("WatchSettingsViewModel/DefaultName"),
                Path = WatchSettings.Default.Path,
                Filters = WatchSettings.Default.Filters,
                Format = WatchSettings.Default.Format,
                Notification = WatchSettings.Default.Notification
            };
            watchSettingsViewModel.PropertyChanged += OnWatchSettingPropertyChanged;
            _WatchSettingsDialog.Model = watchSettingsViewModel;
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
                Format = watchSettingsViewModel.Format,
                Notification = watchSettingsViewModel.Notification
            };
            newWatchSettingsViewModel.PropertyChanged += OnWatchSettingPropertyChanged;
            _WatchSettingsDialog.Model = newWatchSettingsViewModel;
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
            watchSettingsViewModel.Format = newWatchSettingsViewModel.Format;
            watchSettingsViewModel.Notification = newWatchSettingsViewModel.Notification;
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
            if (e.PropertyName != nameof(vm.IsValid)) return;
            _WatchSettingsDialog.IsPrimaryButtonEnabled = vm.IsValid;
        }

        static readonly ResourceLoader _resourceLoader = new();
    }
}
