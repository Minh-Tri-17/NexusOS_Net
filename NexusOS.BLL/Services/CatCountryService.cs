using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NexusOS.BLL.Services
{
    public class CatCountryService : BaseService<CatCountry, CatCountryModel>, ICatCountryService
    {
        #region Infrastructure

        private const string VersionKey = "country:paging:version";
        private readonly ICacheService _cache;
        private readonly CacheHelpers _cacheHelper;

        public CatCountryService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer, ICacheService cache, CacheHelpers cacheHelper)
            : base(context, currentUser, localizer)
        {
            _cache = cache;
            _cacheHelper = cacheHelper;
        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<CatCountryModel>>> GetPaging(FilterModel filter)
        {
            if (DataHelpers.GetBool(filter.IsExport) || _cacheHelper.IsCacheDisabled)
                return await base.GetPaging(filter);

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(filter))));

            for (int attempt = 0; attempt < 2; attempt++)
            {
                string version;
                string cacheKey;

                try
                {
                    version = await _cacheHelper.GetCacheVersionAsync(VersionKey);
                    cacheKey = $"country:paging:{version}:{hash}";

                    var cached = await _cache.GetAsync<PagingResults<CatCountryModel>>(cacheKey);
                    if (cached is not null)
                        return APIResults<PagingResults<CatCountryModel>>.Success(cached, _localizer[Messages.GetListResultSuccess]);
                }
                catch (Exception ex) when (ex is RedisException or TimeoutException)
                {
                    _cacheHelper.DisableCache();
                    return await base.GetPaging(filter);
                }

                var result = await base.GetPaging(filter);

                try
                {
                    // Kiểm tra dữ liệu có bị cập nhật trong lúc GetPaging truy vấn SQL không
                    var currentVersion = await _cacheHelper.GetCacheVersionAsync(VersionKey);
                    if (version != currentVersion)
                        continue;

                    if (result.Result is not null)
                        await _cache.SetAsync(cacheKey, result.Result, TimeSpan.FromMinutes(5));
                }
                catch (Exception ex) when (ex is RedisException or TimeoutException)
                {
                    _cacheHelper.DisableCache();
                }

                return result;
            }

            return await base.GetPaging(filter);
        }

        public override async Task<APIResults<bool>> Create(CatCountryModel request)
        {
            var result = await base.Create(request);

            if (result.Result == true)
                await _cacheHelper.InvalidateCacheAsync(VersionKey);

            return result;
        }

        public override async Task<APIResults<bool>> Update(CatCountryModel request)
        {
            var result = await base.Update(request);

            if (result.Result == true)
                await _cacheHelper.InvalidateCacheAsync(VersionKey);

            return result;
        }

        public override async Task<APIResults<bool>> SoftDelete(List<Guid> ids)
        {
            var result = await base.SoftDelete(ids);

            if (result.Result == true)
                await _cacheHelper.InvalidateCacheAsync(VersionKey);

            return result;
        }

        public override async Task<APIResults<bool>> HardDelete(List<Guid> ids)
        {
            var result = await base.HardDelete(ids);

            if (result.Result == true)
                await _cacheHelper.InvalidateCacheAsync(VersionKey);

            return result;
        }

        public override async Task<APIResults<bool>> Import(IFormFile fileImport)
        {
            var result = await base.Import(fileImport);

            if (result.Result == true)
                await _cacheHelper.InvalidateCacheAsync(VersionKey);

            return result;
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
