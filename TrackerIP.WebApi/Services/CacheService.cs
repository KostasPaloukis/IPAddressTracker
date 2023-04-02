using Microsoft.Extensions.Caching.Memory;
using TrackerIP.Domain.Models;

namespace TrackerIP.WebApi.Services;

public class CacheService : ICacheService
{
    private static readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new() { SlidingExpiration = TimeSpan.FromMinutes(10) };

    public IPDetails? Get(string cacheKey)
    {
        _memoryCache.TryGetValue(cacheKey, out IPDetails? value);
        return value;
    }

    public void Add(string cacheKey, IPDetails value)
    {
        _memoryCache.Set(cacheKey, value, _cacheEntryOptions);
    }

    public void Remove(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }
}
