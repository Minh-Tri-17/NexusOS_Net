
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
            IQueryable<CatWard> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
               .ApplySort()
               .ApplySoftDelete(filter)
               .ApplyCommonFilters(filter);

            var totalCount = 0;
            if (!DataHelpers.GetBool(filter.IsExport))
                totalCount = await query.CountAsync();

            query = query.ApplyPaging(filter);

            var entities = await query.ToListAsync();
            var models = DataHelpers.MappingList<CatWard, CatWardModel>(entities);

            #region Enrich Related Data

            var provinceIds = models.Select(s => s.ProvinceId)
               .Where(s => s.HasValue).Distinct().ToList();

            var provinces = await _context.CatProvinces.AsNoTracking()
                .Where(s => s.IsDelete != true && provinceIds.Contains(s.Id))
                .Select(s => new { s.Id, s.ProvinceName })
                .ToDictionaryAsync(s => s.Id, s => s);

            foreach (var model in models)
            {
                if (provinces.TryGetValue(DataHelpers.GetGuid(model.ProvinceId), out var province))
                    model.ProvinceName = province.ProvinceName;
            }

            #endregion

            var pageResult = new PagingResults<CatWardModel>(models, totalCount, filter.PageIndex, filter.PageSize);

            return APIResults<PagingResults<CatWardModel>>.Success(pageResult, _localizer[Messages.GetListResultSuccess]);
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
