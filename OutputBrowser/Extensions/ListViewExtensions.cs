using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace OutputBrowser.Extensions;

public static class ListViewExtensions
{
    public static ScrollViewer GetScrollViewer(this ListViewBase listView) {
        var child = VisualTreeHelper.GetChild(listView, 0);
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++) {
            var obj = VisualTreeHelper.GetChild(child, i);
            if (obj is not ScrollViewer scrollViewer) continue;
            return scrollViewer;
        }
        return null;
    }

    public static void ScrollToBottom(this ListView listView, bool disableAnimation) {
        var scrollViewer = GetScrollViewer(listView);
        scrollViewer.ChangeView(0.0f, scrollViewer.ExtentHeight, 1.0f, disableAnimation);
    }

    public static void ScrollToBottom(this GridView gridView, bool disableAnimation) {
        var scrollViewer = GetScrollViewer(gridView);
        scrollViewer.ChangeView(0.0f, scrollViewer.ExtentHeight, 1.0f, disableAnimation);
    }
}
