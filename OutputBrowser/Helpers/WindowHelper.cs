using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace OutputBrowser.Helpers;

static partial class WindowHelper
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void ShowWindow(Window window) {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        ShowWindow(hwnd, 0x00000009);
        SetForegroundWindow(hwnd);
    }
}
