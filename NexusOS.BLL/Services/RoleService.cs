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
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                var ids = result.Result.Items.Select(s => s.Id).ToList();

                var rolePermissions = await _context.RolePermissions.AsNoTracking()
                    .Where(s => s.IsDelete != true && s.RoleId.HasValue && ids.Contains(s.RoleId.Value))
                    .ToListAsync();

                foreach (var model in result.Result.Items)
                {
                    model.Permissions = rolePermissions.Where(s => s.RoleId == model.Id).ToList();
                }
            }

            return result;
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
