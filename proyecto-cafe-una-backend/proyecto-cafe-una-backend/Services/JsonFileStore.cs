using System.Text.Json;

namespace proyecto_cafe_una_backend.Services;

public class JsonFileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly string _filePath;

    public JsonFileStore(string fileName)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, fileName);
    }

    public async Task<T> ReadAsync<T>(Func<T> fallbackFactory)
    {
        await _mutex.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                var fallback = fallbackFactory();
                await WriteUnsafeAsync(fallback);
                return fallback;
            }

            await using var readStream = File.OpenRead(_filePath);
            var content = await JsonSerializer.DeserializeAsync<T>(readStream, JsonOptions);
            if (content is not null)
            {
                return content;
            }

            var fallbackValue = fallbackFactory();
            await WriteUnsafeAsync(fallbackValue);
            return fallbackValue;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task WriteAsync<T>(T data)
    {
        await _mutex.WaitAsync();
        try
        {
            await WriteUnsafeAsync(data);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task WriteUnsafeAsync<T>(T data)
    {
        await using var writeStream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(writeStream, data, JsonOptions);
    }
}
