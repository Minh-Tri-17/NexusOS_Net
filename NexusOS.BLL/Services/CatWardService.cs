using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class CatWardService : BaseService<CatWard, CatWardModel>, ICatWardService
    {
        #region Infrastructure

        public CatWardService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<CatWardModel>>> GetPaging(FilterModel filter)
        {
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                #region Enrich Related Data

                var provinceIds = result.Result.Items.Select(s => s.ProvinceId)
                   .Where(s => s.HasValue).Distinct().ToList();

                var provinces = await _context.CatProvinces.AsNoTracking()
                    .Where(s => s.IsDelete != true && provinceIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.ProvinceName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                foreach (var model in result.Result.Items)
                {
                    if (provinces.TryGetValue(DataHelpers.GetGuid(model.ProvinceId), out var province))
                        model.ProvinceName = province.ProvinceName;
                }

                #endregion
            }

            return result;
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
