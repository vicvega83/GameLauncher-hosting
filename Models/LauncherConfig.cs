using System.Text.Json.Serialization;

namespace GameLauncher.Models;

public class LauncherConfig
{
    [JsonPropertyName("game_name")]
    public string GameName { get; set; } = "Game";

    [JsonPropertyName("game_exe")]
    public string GameExe { get; set; } = "Game.exe";

    [JsonPropertyName("version_url")]
    public string VersionUrl { get; set; } = string.Empty;

    [JsonPropertyName("patchnotes_url")]
    public string PatchNotesUrl { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("background_url")]
    public string BackgroundUrl { get; set; } = string.Empty;

    [JsonPropertyName("zoom_speed")]
    public double ZoomSpeed { get; set; } = 0.00015;

    [JsonPropertyName("pan_speed")]
    public double PanSpeed { get; set; } = 0.0001;

    [JsonPropertyName("zoom_min")]
    public double ZoomMin { get; set; } = 1.0;

    [JsonPropertyName("zoom_max")]
    public double ZoomMax { get; set; } = 1.3;
}
