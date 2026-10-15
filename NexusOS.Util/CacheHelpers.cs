using StackExchange.Redis;

namespace NexusOS.Util
{
    public class CacheHelpers
    {
        private readonly IConnectionMultiplexer _redis;
        private int _cacheDisabled;
        public bool IsCacheDisabled => Volatile.Read(ref _cacheDisabled) == 1;

        public CacheHelpers(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> GetCacheVersionAsync(string key)
        {
            var db = _redis.GetDatabase();
            var version = await db.StringGetAsync(key);

            return version.HasValue ? version.ToString() : "0";
        }

        public async Task InvalidateCacheAsync(string key)
        {
            if (IsCacheDisabled)
                return;

            try
            {
                await _redis.GetDatabase().StringIncrementAsync(key);
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                DisableCache();
            }
        }

        public void DisableCache()
        {
            Interlocked.Exchange(ref _cacheDisabled, 1);
        }
    }
}
