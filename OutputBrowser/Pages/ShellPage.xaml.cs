using System;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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

    public ShellPage() {
        _settings = App.GetService<SettingViewModel>();
        InitializeComponent();
        Loaded += ShellPageLoaded;
    }

    void ShellPageLoaded(object sender, RoutedEventArgs e) {
        var firstSelection = _NavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => OutputPage.Type.Equals(i.Tag));
        if (firstSelection != null) {
            firstSelection.Tag = new OutputPage(_settings.Default);
        }
        _NavigationView.SelectedItem = firstSelection;
        _settings.Watches.CollectionChanged += OnWatchesCollectionChanged;
        _settings.Watches.ForEach(AddNavigationViewItem);
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
        } else {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            if (selectedItem.Tag is WatchesSettingViewModel watches) {
                var page = selectedItem.Tag = new OutputPage(watches);
                _ContentFrame.Content = page;
            } else if (selectedItem.Tag is OutputPage page) {
                _ContentFrame.Content = page;
            }
        }
    }

    void OnWatchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.Action == NotifyCollectionChangedAction.Add) {
            e.NewItems.OfType<WatchesSettingViewModel>()
                .ForEach(AddNavigationViewItem);
        } else if (e.Action == NotifyCollectionChangedAction.Remove) {
            e.OldItems.OfType<WatchesSettingViewModel>()
                .ForEach(RemoveNavigationViewItem);
        }
    }

    void AddNavigationViewItem(WatchesSettingViewModel watch) {
        _NavigationView.MenuItems.Add(new NavigationViewItem {
            Content = watch.Name,
            Tag = watch
        });
    }

    void RemoveNavigationViewItem(WatchesSettingViewModel watch) {
        _NavigationView.MenuItems.OfType<NavigationViewItem>()
            .Where(i => i.Name == watch.Name)
            .ForEach(i => _NavigationView.MenuItems.Remove(i));
    }

    readonly SettingViewModel _settings;
}
