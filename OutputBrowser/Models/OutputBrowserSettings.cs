using System.Collections.Generic;

namespace OutputBrowser.Models;

public class OutputBrowserSettings
{
    public string Theme { get; set; } = "Default";
    public string Backdrop { get; set; } = "Mica";

    public WatchSettings Default { get; set; }
    public Dictionary<string, WatchesSettings> Watches { get; set; } = [];
}
