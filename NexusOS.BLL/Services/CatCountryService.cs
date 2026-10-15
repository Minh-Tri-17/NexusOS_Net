using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;
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
            if (DataHelpers.GetBool(filter.IsExport))
                return await base.GetPaging(filter);

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(filter))));

            for (int attempt = 0; attempt < 2; attempt++)
            {
                var version = await _cacheHelper.GetCacheVersionAsync(VersionKey);
                var cacheKey = $"country:paging:{version}:{hash}";

                var cached = await _cache.GetAsync<PagingResults<CatCountryModel>>(cacheKey);
                if (cached is not null)
                    return APIResults<PagingResults<CatCountryModel>>.Success(cached, _localizer[Messages.GetListResultSuccess]);

                var result = await base.GetPaging(filter);

                // Kiểm tra dữ liệu có bị cập nhật trong lúc GetPaging truy vấn SQL không
                var currentVersion = await _cacheHelper.GetCacheVersionAsync(VersionKey);
                if (version != currentVersion)
                    continue;

                if (result.Result is not null)
                    await _cache.SetAsync(cacheKey, result.Result, TimeSpan.FromMinutes(5));

                return result;
            }

            return await base.GetPaging(filter);
        }

        public override async Task<APIResults<bool>> Update(CatCountryModel request)
        {
            var result = await base.Update(request);

            if (result.Result == true)
                await _cacheHelper.InvalidatePagingCacheAsync(VersionKey);

            return result;
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
