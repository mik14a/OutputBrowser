using System.Collections.Generic;

namespace OutputBrowser.Models;

public class OutputBrowserSettings
{
    public string Theme { get; set; } = "Default";
    public string Backdrop { get; set; } = "Mica";
    public string Viewer { get; set; } = "BuiltIn";

    public WatchSettings Default { get; set; }
    public Dictionary<string, WatchesSettings> Watches { get; set; } = [];
}
