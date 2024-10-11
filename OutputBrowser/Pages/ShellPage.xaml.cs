using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace OutputBrowser.Pages;

/// <summary>
/// A shell page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ShellPage : Page
{
    public UIElement AppTitleBar => _AppTitleBar;

    public ShellPage() {
        InitializeComponent();
    }
}
