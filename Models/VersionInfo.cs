using System.Text.Json.Serialization;

namespace GameLauncher.Models;

public class VersionInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("filesize")]
    public long Filesize { get; set; }

    [JsonPropertyName("changelog")]
    public string Changelog { get; set; } = string.Empty;
}
