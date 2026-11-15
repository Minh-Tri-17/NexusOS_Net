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

        protected override IQueryable<CatDepartmentModel> ProjectToModel(IQueryable<CatDepartment> query)
        {
            return query.Select(s => new CatDepartmentModel
            {
                Id = s.Id,
                DepartmentCode = s.DepartmentCode,
                DepartmentName = s.DepartmentName,
                Note = s.Note,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,

                ManagerName = s.Manager != null && s.Manager.IsDelete != true
                    ? s.Manager.EmployeeName : null
            });
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
