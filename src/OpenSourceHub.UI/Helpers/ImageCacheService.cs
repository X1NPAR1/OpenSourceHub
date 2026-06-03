using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace OpenSourceHub.UI.Helpers;

public sealed class ImageCacheService
{
    private static readonly ImageCacheService _instance = new();
    public static ImageCacheService Instance => _instance;

    private readonly ConcurrentDictionary<string, BitmapImage?> _cache = new();
    private readonly HttpClient _http = new();
    private readonly SemaphoreSlim _throttle = new(4, 4);

    public async Task<BitmapImage?> LoadAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (_cache.TryGetValue(url, out var cached)) return cached;

        await _throttle.WaitAsync();
        try
        {
            if (_cache.TryGetValue(url, out cached)) return cached;

            var bytes = await _http.GetByteArrayAsync(url);
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = new MemoryStream(bytes);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.DecodePixelWidth = 128;
            img.EndInit();
            img.Freeze();

            _cache[url] = img;
            return img;
        }
        catch
        {
            _cache[url] = null;
            return null;
        }
        finally
        {
            _throttle.Release();
        }
    }
}
