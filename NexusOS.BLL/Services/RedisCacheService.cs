using Microsoft.Extensions.Caching.Distributed;
using NexusOS.BLL.Interfaces;
using System.Text.Json;

namespace NexusOS.BLL.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var json = await _cache.GetStringAsync(key, ct);

            if (json is null)
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(key, json, options, ct);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            await _cache.RemoveAsync(key, ct);
        }
    }
}
