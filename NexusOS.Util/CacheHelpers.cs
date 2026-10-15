using StackExchange.Redis;

namespace NexusOS.Util
{
    public class CacheHelpers
    {
        private readonly IConnectionMultiplexer _redis;

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

        public async Task InvalidatePagingCacheAsync(string key)
        {
            var db = _redis.GetDatabase();
            await db.StringIncrementAsync(key);
        }
    }
}
