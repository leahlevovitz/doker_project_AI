using System.Text.Json;
using StackExchange.Redis;

namespace ProductCatalogService.Cache;

public class GiftCacheService
{
    private readonly IDatabase _redis;
    private readonly ILogger<GiftCacheService> _logger;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);
    private const string Prefix = "gift:catalog:";

    public GiftCacheService(IConnectionMultiplexer redis, ILogger<GiftCacheService> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _redis.StringGetAsync($"{Prefix}{key}");
        if (value.IsNullOrEmpty)
        {
            _logger.LogInformation("[ProductCatalog] CACHE MISS key={Key}", key);
            return default;
        }
        _logger.LogInformation("[ProductCatalog] CACHE HIT key={Key}", key);
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        await _redis.StringSetAsync($"{Prefix}{key}", JsonSerializer.Serialize(value), Ttl);
        _logger.LogInformation("[ProductCatalog] CACHE SET key={Key} ttl={Ttl}", key, Ttl);
    }

    public async Task InvalidateAsync(string key)
    {
        await _redis.KeyDeleteAsync($"{Prefix}{key}");
        _logger.LogInformation("[ProductCatalog] CACHE INVALIDATED key={Key}", key);
    }

    public async Task InvalidateAllAsync()
    {
        // Invalidate the "all gifts" list cache
        await _redis.KeyDeleteAsync($"{Prefix}all");
        _logger.LogInformation("[ProductCatalog] CACHE INVALIDATED key=all");
    }
}
