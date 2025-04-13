using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace OutputBrowser.Helpers;

static partial class WindowHelper
{
    public static void UpdateTitleBar(ElementTheme theme) {
        if (!App.MainWindow.ExtendsContentIntoTitleBar) return;

        if (theme == ElementTheme.Default) {
            var uiSettings = new UISettings();
            var background = uiSettings.GetColorValue(UIColorType.Background);
            theme = background == Colors.White ? ElementTheme.Light : ElementTheme.Dark;
        }

        if (theme == ElementTheme.Default) {
            theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
        }

        App.MainWindow.AppWindow.TitleBar.ButtonForegroundColor = theme switch {
            ElementTheme.Dark => Colors.White,
            ElementTheme.Light => Colors.Black,
            _ => Colors.Transparent
        };

        App.MainWindow.AppWindow.TitleBar.ButtonHoverForegroundColor = theme switch {
            ElementTheme.Dark => Colors.White,
            ElementTheme.Light => Colors.Black,
            _ => Colors.Transparent
        };

        App.MainWindow.AppWindow.TitleBar.ButtonHoverBackgroundColor = theme switch {
            ElementTheme.Dark => Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF),
            ElementTheme.Light => Color.FromArgb(0x33, 0x00, 0x00, 0x00),
            _ => Colors.Transparent
        };

        App.MainWindow.AppWindow.TitleBar.ButtonPressedBackgroundColor = theme switch {
            ElementTheme.Dark => Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF),
            ElementTheme.Light => Color.FromArgb(0x66, 0x00, 0x00, 0x00),
            _ => Colors.Transparent
        };
        App.MainWindow.AppWindow.TitleBar.BackgroundColor = Colors.Transparent;
    }

    public static void ShowWindow(Window window) {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        ShowWindow(hWnd, 0x00000009);
        SetForegroundWindow(hWnd);
    }

    const int WAINACTIVE = 0x00;
    const int WAACTIVE = 0x01;
    const int WMACTIVATE = 0x0006;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetActiveWindow();
}
