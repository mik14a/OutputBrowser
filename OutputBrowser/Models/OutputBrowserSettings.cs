using System.Collections.Generic;

namespace OutputBrowser.Models;

public class OutputBrowserSettings
{
    public WatchSettings Default { get; set; }

    public Dictionary<string, WatchesSettings> Watches { get; set; } = [];
}
