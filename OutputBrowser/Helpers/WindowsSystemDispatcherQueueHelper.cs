using System;
using System.Runtime.InteropServices;

namespace OutputBrowser.Helpers;

partial class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        public int dwSize;
        public int threadType;
        public int apartmentType;
    }

    [LibraryImport("CoreMessaging.dll")]
    private static unsafe partial int CreateDispatcherQueueController(DispatcherQueueOptions options, IntPtr* instance);

    IntPtr _dispatcherQueueController = IntPtr.Zero;

    public void EnsureWindowsSystemDispatcherQueueController() {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null) {
            // one already exists, so we'll just use it.
            return;
        }

        if (_dispatcherQueueController == IntPtr.Zero) {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf<DispatcherQueueOptions>();
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            unsafe {
                IntPtr dispatcherQueueController;
                _ = CreateDispatcherQueueController(options, &dispatcherQueueController);
                _dispatcherQueueController = dispatcherQueueController;
            }
        }
    }
}
