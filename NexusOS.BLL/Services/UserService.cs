using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class UserService : BaseService<User, UserModel>, IUserService
    {
        #region Infrastructure

        private readonly IPermissionService _permissionService;

        public UserService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer, IPermissionService permissionService) : base(context, currentUser, localizer)
        {
            _permissionService = permissionService;
        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<UserModel>> GetOne(Guid id)
        {
            var result = await base.GetOne(id);
            if (result.IsSuccess && result.Result != null)
            {
                var roleIds = await _context.UserRoles.AsNoTracking()
                    .Where(s => s.IsDelete != true && s.UserId == id && s.RoleId.HasValue)
                    .Select(s => s.RoleId!.Value.ToString())
                    .ToListAsync();

                result.Result.RoleIds = roleIds;
            }

            return result;
        }

        public override async Task<APIResults<PagingResults<UserModel>>> GetPaging(FilterModel filter)
        {
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                var userIds = result.Result.Items.Select(s => s.Id).ToList();

                var userRoles = await _context.UserRoles.AsNoTracking()
                    .Where(s => s.IsDelete != true && s.UserId.HasValue && userIds.Contains(s.UserId.Value) && s.RoleId.HasValue)
                    .ToListAsync();

                foreach (var model in result.Result.Items)
                {
                    model.RoleIds = userRoles
                        .Where(s => s.UserId == model.Id && s.RoleId.HasValue)
                        .Select(s => s.RoleId!.Value.ToString())
                        .ToList();
                }
            }

            return result;
        }

        protected override IQueryable<UserModel>? ProjectToModel(IQueryable<User> query)
        {
            return query.Select(u => new UserModel
            {
                Id = u.Id,
                StoreId = u.StoreId,
                EmployeeId = u.EmployeeId,
                UserCode = u.UserCode,
                Username = u.Username,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                UserStatus = u.UserStatus,
                UserType = u.UserType,
                LastLoginAt = u.LastLoginAt,
                FailedLoginCount = u.FailedLoginCount,
                PasswordExpiredAt = u.PasswordExpiredAt,
                RequirePasswordChange = u.RequirePasswordChange,
                TwoFactorEnabled = u.TwoFactorEnabled,
                Note = u.Note,
                CreatedAt = u.CreatedAt,
                CreatedBy = u.CreatedBy,
                UpdatedAt = u.UpdatedAt,
                UpdatedBy = u.UpdatedBy,

                EmployeeName = u.Employee != null ? u.Employee.EmployeeName : null,
            });
        }

        protected override Task BeforeSaveAsync(UserModel request, User entity, bool isNew)
        {
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var hash = PasswordHasher.HashPassword(request.Password);
                request.PasswordHash = hash;
                entity.PasswordHash = hash;
            }
            else if (!isNew)
            {
                request.PasswordHash = entity.PasswordHash;
            }

            return Task.CompletedTask; // Hoàn thành phương thức trả về Task khi không cần dùng await.
        }

        protected override async Task AfterSaveAsync(UserModel request, User entity)
        {
            var existingRoles = _context.UserRoles.Where(x => x.UserId == entity.Id);
            _context.UserRoles.RemoveRange(existingRoles);

            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var userRoles = request.RoleIds
                    .Select(roleId => DataHelpers.GetGuid(roleId))
                    .Where(s => s != Guid.Empty)
                    .Distinct()
                    .Select(s => new UserRole
                    {
                        Id = Guid.CreateVersion7(),
                        UserId = entity.Id,
                        RoleId = s,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _currentUser.UserId
                    });

                await _context.UserRoles.AddRangeAsync(userRoles);
            }

            await _permissionService.InvalidatePermissionCache();
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
