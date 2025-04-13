using Microsoft.UI; // required to support Window.As<ICompositionSupportsSystemBackdrop>()
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using OutputBrowser.Helpers;
using WinRT;
using WinUIEx;

namespace OutputBrowser;

/// <summary>
/// A main window that can be used on its own or navigated to within a Frame.
/// </summary>
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
        if (_acrylicCustomController != null) {
            _acrylicCustomController.Dispose();
            _acrylicCustomController = null;
        }
        Closed -= OnWindowClosed;
        _configurationSource = null;

        if (backdrop == Controls.SystemBackdrop.Mica) {
            SystemBackdrop = new MicaBackdrop { Kind = MicaKind.Base };
        } else if (backdrop == Controls.SystemBackdrop.MicaAlt) {
            SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
        } else if (backdrop == Controls.SystemBackdrop.Acrylic) {
            SystemBackdrop = new DesktopAcrylicBackdrop();
        } else if (backdrop == Controls.SystemBackdrop.AcrylicCustom) {
            if (DesktopAcrylicController.IsSupported()) {
                SystemBackdrop = null;
                _configurationSource = new SystemBackdropConfiguration();
                Closed += OnWindowClosed;

                _configurationSource.IsInputActive = true;
                _acrylicCustomController = new DesktopAcrylicController {
                    Kind = DesktopAcrylicKind.Thin,
                    TintColor = Colors.Transparent
                };
                _acrylicCustomController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                _acrylicCustomController.SetSystemBackdropConfiguration(_configurationSource);
            }
        }
    }

    void OnWindowClosed(object sender, WindowEventArgs args) {
        _acrylicCustomController?.Dispose();
        _acrylicCustomController = null;
        _configurationSource = null;
    }

    readonly WindowsSystemDispatcherQueueHelper _wsdqHelper;
    DesktopAcrylicController _acrylicCustomController;
    SystemBackdropConfiguration _configurationSource;
}
