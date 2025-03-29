using System.Collections.Generic;

namespace OutputBrowser.Models;

public class WatchesSettings
{
    public string Name { get; set; }
    public List<WatchSettings> Watches { get; set; } = [];
}
