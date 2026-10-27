
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class EmployeeService : BaseService<Employee, EmployeeModel>, IEmployeeService
    {
        #region Infrastructure

        public EmployeeService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<EmployeeModel>>> GetPaging(FilterModel filter)
        {
            IQueryable<Employee> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                  .ApplySort()
                  .ApplySoftDelete(filter)
                  .ApplyCommonFilters(filter);

            var totalCount = 0;
            if (!DataHelpers.GetBool(filter.IsExport))
                totalCount = await query.CountAsync();

            query = query.ApplyPaging(filter);

            var entities = await query.ToListAsync();
            var models = DataHelpers.MappingList<Employee, EmployeeModel>(entities);

            #region Enrich Related Data

            var countryIds = models.Select(s => s.CountryId)
                .Where(s => s.HasValue).Distinct().ToList();

            var countries = await _context.CatCountries.AsNoTracking()
                .Where(s => s.IsDelete != true && countryIds.Contains(s.Id))
                .Select(s => new { s.Id, s.CountryName })
                .ToDictionaryAsync(s => s.Id, s => s);

            var provinceIds = models.Select(s => s.ProvinceId)
                .Where(s => s.HasValue).Distinct().ToList();

            var provinces = await _context.CatProvinces.AsNoTracking()
                .Where(s => s.IsDelete != true && provinceIds.Contains(s.Id))
                .Select(s => new { s.Id, s.ProvinceName })
                .ToDictionaryAsync(s => s.Id, s => s);

            var wardIds = models.Select(s => s.WardId)
                .Where(s => s.HasValue).Distinct().ToList();

            var wards = await _context.CatWards.AsNoTracking()
                .Where(s => s.IsDelete != true && wardIds.Contains(s.Id))
                .Select(s => new { s.Id, s.WardName })
                .ToDictionaryAsync(s => s.Id, s => s);

            foreach (var model in models)
            {
                if (countries.TryGetValue(DataHelpers.GetGuid(model.CountryId), out var country))
                    model.CountryName = country.CountryName;

                if (provinces.TryGetValue(DataHelpers.GetGuid(model.ProvinceId), out var province))
                    model.ProvinceName = province.ProvinceName;

                if (wards.TryGetValue(DataHelpers.GetGuid(model.WardId), out var ward))
                    model.WardName = ward.WardName;
            }

            #endregion

            var pageResult = new PagingResults<EmployeeModel>(models, totalCount, filter.PageIndex, filter.PageSize);

            return APIResults<PagingResults<EmployeeModel>>.Success(pageResult, _localizer[Messages.GetListResultSuccess]);
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
