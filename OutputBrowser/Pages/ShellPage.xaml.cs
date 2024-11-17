using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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
        InitializeComponent();
    }

    void NavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
        if (args.SelectedItem == null) {
            return;  // Clear selection.
        } else if (args.IsSettingsSelected) {
            _ContentFrame.Navigate(typeof(SettingPage), args.RecommendedNavigationTransitionInfo);
        } else {
            var frameNavigationOptions = new FrameNavigationOptions {
                TransitionInfoOverride = args.RecommendedNavigationTransitionInfo,
                IsNavigationStackEnabled = false
            };
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            var page = selectedItem.Tag switch {
                "OutputPage" => typeof(OutputPage),
                _ => throw new InvalidOperationException($"Invalid page type {selectedItem.Tag}")
            };
            _ContentFrame.NavigateToType(page, null, frameNavigationOptions);
        }
    }
}
