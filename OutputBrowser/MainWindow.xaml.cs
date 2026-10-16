using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using OutputBrowser.Helpers;
using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()
using WinUIEx;

namespace OutputBrowser;

/// <summary>
/// A main window that can be used on its own or navigated to within a Frame.
/// </summary>
/// <remarks>
/// Custom acrylic follows the controller procedure:
/// https://learn.microsoft.com/windows/apps/develop/ui/system-backdrops#advanced-how-to-use-a-system-backdrop-controller
/// </remarks>
public sealed partial class MainWindow : WindowEx
{
    public MainWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        _wsdqHelper = new WindowsSystemDispatcherQueueHelper();
        _wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
    }

    public void SetTheme(ElementTheme value) {
        if (Content is FrameworkElement root) {
            root.RequestedTheme = value;
        }
    }

    public void SetBackdrop(Controls.SystemBackdrop backdrop) {
        _acrylicCustomController?.Dispose();
        _acrylicCustomController = null;
        Closed -= OnWindowClosed;
        if (Content is FrameworkElement attached) {
            attached.ActualThemeChanged -= OnRootThemeChanged;
        }
        _configurationSource = null;

        if (backdrop == Controls.SystemBackdrop.Mica) {
            SystemBackdrop = new MicaBackdrop { Kind = MicaKind.Base };
        } else if (backdrop == Controls.SystemBackdrop.MicaAlt) {
            SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
        } else if (backdrop == Controls.SystemBackdrop.Acrylic) {
            SystemBackdrop = new DesktopAcrylicBackdrop();
        } else if (backdrop == Controls.SystemBackdrop.AcrylicCustom) {
            if (DesktopAcrylicController.IsSupported()) {
                var root = (FrameworkElement)Content;
                SystemBackdrop = null;
                _configurationSource = new SystemBackdropConfiguration {
                    // Pinned always-on-top stays visible while another window is used.
                    IsInputActive = true,
                    Theme = root.ActualTheme switch {
                        ElementTheme.Dark => SystemBackdropTheme.Dark,
                        ElementTheme.Light => SystemBackdropTheme.Light,
                        _ => SystemBackdropTheme.Default,
                    },
                };
                Closed += OnWindowClosed;
                root.ActualThemeChanged += OnRootThemeChanged;

                // These values disable the controller's automatic light and dark recipes.
                _acrylicCustomController = new DesktopAcrylicController {
                    Kind = DesktopAcrylicKind.Thin,
                    TintColor = Colors.Transparent,
                    FallbackColor = Colors.Transparent,
                    TintOpacity = 0.0f,
                    LuminosityOpacity = 0.0f,
                };
                _acrylicCustomController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicCustomController.SetSystemBackdropConfiguration(_configurationSource);
            } else {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        }
    }

    void OnWindowClosed(object sender, WindowEventArgs args) {
        _acrylicCustomController?.Dispose();
        _acrylicCustomController = null;
        Closed -= OnWindowClosed;
        if (Content is FrameworkElement root) {
            root.ActualThemeChanged -= OnRootThemeChanged;
        }
        _configurationSource = null;
    }

    void OnRootThemeChanged(FrameworkElement sender, object args) {
        _configurationSource.Theme = sender.ActualTheme switch {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            _ => SystemBackdropTheme.Default,
        };
    }

    readonly WindowsSystemDispatcherQueueHelper _wsdqHelper;
    DesktopAcrylicController _acrylicCustomController;
    SystemBackdropConfiguration _configurationSource;
}
