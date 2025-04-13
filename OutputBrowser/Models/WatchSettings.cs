using System.Text.Json.Serialization;

namespace OutputBrowser.Models;

public class WatchSettings
{
    public static readonly string FilePath = "{{path}}";
    public static readonly string FileName = "{{name}}";

    public static readonly WatchSettings Default = new() {
        Filters = "*.png",
        Format = "{{path}}",
        Notification = false
    };

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte[] Icon { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Filters { get; set; }
    public string Format { get; set; }
    public bool Notification { get; set; }
}
