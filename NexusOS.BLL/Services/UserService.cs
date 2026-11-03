
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

        public override async Task<APIResults<PagingResults<UserModel>>> GetPaging(FilterModel filter)
        {
            IQueryable<User> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .ApplySort()
                .ApplySoftDelete(filter)
                .ApplyCommonFilters(filter);

            var totalCount = 0;
            if (!DataHelpers.GetBool(filter.IsExport))
                totalCount = await query.CountAsync();

            query = query.ApplyPaging(filter);

            var entities = await query.ToListAsync();
            var models = DataHelpers.MappingList<User, UserModel>(entities);

            #region Enrich Related Data

            var employeeIds = models.Select(s => s.EmployeeId)
               .Where(s => s.HasValue).Distinct().ToList();

            var employees = await _context.Employees.AsNoTracking()
                .Where(s => s.IsDelete != true && employeeIds.Contains(s.Id))
                .Select(s => new { s.Id, s.EmployeeName })
                .ToDictionaryAsync(s => s.Id, s => s);

            foreach (var model in models)
            {
                if (employees.TryGetValue(DataHelpers.GetGuid(model.EmployeeId), out var employee))
                    model.EmployeeName = employee.EmployeeName;
            }

            #endregion

            var pageResult = new PagingResults<UserModel>(models, totalCount, filter.PageIndex, filter.PageSize);

            return APIResults<PagingResults<UserModel>>.Success(pageResult, _localizer[Messages.GetListResultSuccess]);
        }

        protected override Task BeforeSaveAsync(UserModel request, User entity, bool isNew)
        {
            if (!string.IsNullOrWhiteSpace(request.Password))
                request.PasswordHash = PasswordHasher.HashPassword(request.Password);
            else if (!isNew)
                request.PasswordHash = entity.PasswordHash;

            return Task.CompletedTask; // Hoàn thành phương thức trả về Task khi không cần dùng await.
        }

        protected override async Task AfterSaveAsync(UserModel request, User entity)
        {
            var existingRoles = _context.UserRoles.Where(x => x.UserId == entity.Id);
            _context.UserRoles.RemoveRange(existingRoles);

            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var userRoles = request.RoleIds.Select(roleId => new UserRole
                {
                    UserId = entity.Id,
                    RoleId = DataHelpers.GetGuid(roleId)
                });

                await _context.UserRoles.AddRangeAsync(userRoles);
            }
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
