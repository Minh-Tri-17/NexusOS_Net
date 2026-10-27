
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
            IQueryable<CatProvince> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
               .ApplySort()
               .ApplySoftDelete(filter)
               .ApplyCommonFilters(filter);

            var totalCount = 0;
            if (!DataHelpers.GetBool(filter.IsExport))
                totalCount = await query.CountAsync();

            query = query.ApplyPaging(filter);

            var entities = await query.ToListAsync();
            var models = DataHelpers.MappingList<CatProvince, CatProvinceModel>(entities);

            #region Enrich Related Data

            var countryIds = models.Select(s => s.CountryId)
                .Where(s => s.HasValue).Distinct().ToList();

            var countries = await _context.CatCountries.AsNoTracking()
                .Where(s => s.IsDelete != true && countryIds.Contains(s.Id))
                .Select(s => new { s.Id, s.CountryName })
                .ToDictionaryAsync(s => s.Id, s => s);

            foreach (var model in models)
            {
                if (countries.TryGetValue(DataHelpers.GetGuid(model.CountryId), out var country))
                    model.CountryName = country.CountryName;
            }

            #endregion

            var pageResult = new PagingResults<CatProvinceModel>(models, totalCount, filter.PageIndex, filter.PageSize);

            return APIResults<PagingResults<CatProvinceModel>>.Success(pageResult, _localizer[Messages.GetListResultSuccess]);
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
