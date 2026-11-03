
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class RoleService : BaseService<Role, RoleModel>, IRoleService
    {
        #region Infrastructure

        public RoleService(NexusOsContext context, ICurrentUserService currentRole,
            IStringLocalizer<SharedResource> localizer) : base(context, currentRole, localizer)
        {

        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<RoleModel>>> GetPaging(FilterModel filter)
        {
            IQueryable<Role> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .ApplySort()
                .ApplySoftDelete(filter)
                .ApplyCommonFilters(filter);

            var totalCount = 0;
            if (!DataHelpers.GetBool(filter.IsExport))
                totalCount = await query.CountAsync();

            query = query.ApplyPaging(filter);

            var entities = await query.ToListAsync();
            var models = DataHelpers.MappingList<Role, RoleModel>(entities);

            #region Enrich Related Data

            var ids = models.Select(s => s.Id).ToList();

            var rolePermissions = await _context.RolePermissions.AsNoTracking()
                .Where(s => s.IsDelete != true && s.RoleId.HasValue && ids.Contains(s.RoleId.Value))
                .ToListAsync();

            foreach (var model in models)
            {
                model.Permissions = rolePermissions.Where(s => s.RoleId == model.Id).ToList();
            }

            #endregion

            var pageResult = new PagingResults<RoleModel>(models, totalCount, filter.PageIndex, filter.PageSize);

            return APIResults<PagingResults<RoleModel>>.Success(pageResult, _localizer[Messages.GetListResultSuccess]);
        }

        protected override async Task AfterSaveAsync(RoleModel request, Role entity)
        {
            var existingPermissions = _context.RolePermissions.Where(x => x.RoleId == entity.Id);
            _context.RolePermissions.RemoveRange(existingPermissions);

            if (request.Permissions != null)
            {
                var rolePermissions = request.Permissions.Select(permission => new RolePermission
                {
                    Id = Guid.CreateVersion7(),
                    RoleId = entity.Id,
                    PermissionCode = permission.PermissionCode,
                    PermissionValue = permission.PermissionValue,
                    PermissionScope = permission.PermissionScope,
                    IsGranted = permission.IsGranted
                });

                await _context.RolePermissions.AddRangeAsync(rolePermissions);
            }
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
