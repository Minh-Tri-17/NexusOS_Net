using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class CatProvinceService : BaseService<CatProvince, CatProvinceModel>, ICatProvinceService
    {
        #region Infrastructure

        public CatProvinceService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<CatProvinceModel>>> GetPaging(FilterModel filter)
        {
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                #region Enrich Related Data

                var countryIds = result.Result.Items.Select(s => s.CountryId)
                    .Where(s => s.HasValue).Distinct().ToList();

                var countries = await _context.CatCountries.AsNoTracking()
                    .Where(s => s.IsDelete != true && countryIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.CountryName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                foreach (var model in result.Result.Items)
                {
                    if (countries.TryGetValue(DataHelpers.GetGuid(model.CountryId), out var country))
                        model.CountryName = country.CountryName;
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
