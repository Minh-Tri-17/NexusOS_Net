using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class CatDepartmentService : BaseService<CatDepartment, CatDepartmentModel>, ICatDepartmentService
    {
        #region Infrastructure

        public CatDepartmentService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<CatDepartmentModel>>> GetPaging(FilterModel filter)
        {
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                #region Enrich Related Data

                var managerIds = result.Result.Items.Select(s => s.ManagerId)
                   .Where(s => s.HasValue).Distinct().ToList();

                var managers = await _context.Employees.AsNoTracking()
                    .Where(s => s.IsDelete != true && managerIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.EmployeeName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                foreach (var model in result.Result.Items)
                {
                    if (managers.TryGetValue(DataHelpers.GetGuid(model.ManagerId), out var manager))
                        model.ManagerName = manager.EmployeeName;
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
