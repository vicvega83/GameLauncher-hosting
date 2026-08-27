using System.Net;
using System.Text.Json;
using GameLauncher.Models;

namespace GameLauncher.Services;

public class DataService
{
    private readonly HttpClient _httpClient;

    public DataService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<LauncherConfig?> LoadLauncherConfig(string url)
    {
        try
        {
            string json = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<LauncherConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PatchNoteEntry>?> LoadPatchNotes(string url)
    {
        try
        {
            string json = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<List<PatchNoteEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<VersionInfo?> LoadVersionInfo(string url)
    {
        try
        {
            string json = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<VersionInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> DownloadFile(string url)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(url);
        }
        catch
        {
            return null;
        }
    }

    public async Task DownloadUpdate(string url, string savePath, Action<int, int>? progressCallback = null)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        long totalBytes = response.Content.Headers.ContentLength ?? 0;
        long downloaded = 0;

        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        byte[] buffer = new byte[81920];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            downloaded += bytesRead;
            if (totalBytes > 0 && progressCallback != null)
            {
                progressCallback(Convert.ToInt32((downloaded * 100) / totalBytes), (int)downloaded);
            }
        }
    }
}
