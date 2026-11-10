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

        public UserService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

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
                var employeeIds = result.Result.Items.Select(s => s.EmployeeId)
                   .Where(s => s.HasValue).Distinct().ToList();

                var employees = await _context.Employees.AsNoTracking()
                    .Where(s => s.IsDelete != true && employeeIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.EmployeeName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var userIds = result.Result.Items.Select(s => s.Id).ToList();

                var userRoles = await _context.UserRoles.AsNoTracking()
                    .Where(s => s.IsDelete != true && s.UserId.HasValue && userIds.Contains(s.UserId.Value) && s.RoleId.HasValue)
                    .ToListAsync();

                foreach (var model in result.Result.Items)
                {
                    if (employees.TryGetValue(DataHelpers.GetGuid(model.EmployeeId), out var employee))
                        model.EmployeeName = employee.EmployeeName;

                    model.RoleIds = userRoles
                        .Where(s => s.UserId == model.Id && s.RoleId.HasValue)
                        .Select(s => s.RoleId!.Value.ToString())
                        .ToList();
                }
            }

            return result;
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
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
