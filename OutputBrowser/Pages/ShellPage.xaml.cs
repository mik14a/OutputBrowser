using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using OutputBrowser.Models;
using OutputBrowser.ViewModels;

namespace OutputBrowser.Pages;

/// <summary>
/// A shell page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ShellPage : Page
{
    public UIElement AppTitleBar => _AppTitleBar;
    public NavigationView NavigationView => _NavigationView;
    public Frame ContentFrame => _ContentFrame;

    public ObservableCollection<WatchesSettingsViewModel> Watches { get; } = [];

    public ShellPage() {
        _settings = App.GetService<SettingViewModel>();
        InitializeComponent();
        Loaded += ShellPageLoaded;
    }

    void ShellPageLoaded(object sender, RoutedEventArgs e) {

        var defaultWatched = new WatchesSettingsViewModel(null, new WatchesSettings {
            Icon = "Accept", Name = _resourceLoader.GetString("OutputBrowserNavigationViewItem/Content"),
        }) {
            Page = new OutputPage(_settings.Default)
        };
        Watches.Add(defaultWatched);
        _NavigationView.SelectedItem = defaultWatched;
        _settings.Watches.CollectionChanged += OnWatchesCollectionChanged;
        _settings.Watches.ForEach(Watches.Add);
    }

    public void Navigate(Type type, object parameter) {
        var frameNavigationOptions = new FrameNavigationOptions {
            IsNavigationStackEnabled = true
        };
        _ContentFrame.NavigateToType(type, parameter, frameNavigationOptions);
    }

    public void GoBack() {
        if (_ContentFrame.CanGoBack) {
            _ContentFrame.GoBack();
        }
    }

    void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
        if (args.SelectedItem == null) {
            return;  // Clear selection.
        } else if (args.IsSettingsSelected) {
            _ContentFrame.Navigate(typeof(SettingPage), args.RecommendedNavigationTransitionInfo);
        } else if (args.SelectedItem is WatchesSettingsViewModel watches) {
            _ContentFrame.Content = watches.Page ??= new OutputPage(watches);
        }
    }

    void OnWatchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.Action == NotifyCollectionChangedAction.Add) {
            e.NewItems.OfType<WatchesSettingsViewModel>().ForEach(Watches.Add);
        } else if (e.Action == NotifyCollectionChangedAction.Remove) {
            e.OldItems.OfType<WatchesSettingsViewModel>().ForEach(watches => Watches.Remove(watches));
        }
    }

    readonly SettingViewModel _settings;
    static readonly ResourceLoader _resourceLoader = new();
}
