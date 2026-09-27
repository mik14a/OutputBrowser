using OutputBrowser.ViewModels;
using Windows.Graphics;
using WinUIEx;

namespace OutputBrowser.Browser;

public sealed partial class ImageBrowserWindow : WindowEx
{
    public ImageBrowserWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(_AppTitleBar);
        AppWindow.Resize(new SizeInt32(1024, 768));
    }

    public void SetOutput(OutputViewModel output) {
        _Root.DataContext = output;
        Title = output?.DisplayName ?? nameof(OutputBrowser);
    }
}
