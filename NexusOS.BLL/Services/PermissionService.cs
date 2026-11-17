using Microsoft.EntityFrameworkCore;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class PermissionService : IPermissionService
    {
        #region Infrastructure

        private const string VersionKey = "permission:version";
        private readonly NexusOsContext _context;
        private readonly IRedisCacheService _cache;
        private readonly CacheHelpers _cacheHelpers;

        public PermissionService(
            NexusOsContext context,
            IRedisCacheService cache,
            CacheHelpers cacheHelpers)
        {
            _context = context;
            _cache = cache;
            _cacheHelpers = cacheHelpers;
        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        public async Task<HashSet<string>> GetUserPermissions(Guid userId)
        {
            if (userId == Guid.Empty)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string? cacheKey = null;

            if (!_cacheHelpers.IsCacheDisabled)
            {
                try
                {
                    var version = await _cacheHelpers.GetCacheVersionAsync(VersionKey);
                    cacheKey = $"permissions:v2:{version}:user:{userId}";

                    var cached = await _cache.GetAsync<List<string>>(cacheKey);
                    if (cached != null)
                        return new HashSet<string>(cached, StringComparer.OrdinalIgnoreCase); // OrdinalIgnoreCase giúp xác định không phân biệt hoa thường
                }
                catch
                {
                    _cacheHelpers.DisableCache();
                }
            }

            var rawPermissions = await
                (from ur in _context.UserRoles.AsNoTracking()
                 join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                 join rp in _context.RolePermissions.AsNoTracking() on r.Id equals rp.RoleId
                 where ur.UserId == userId
                     && ur.IsDelete != true
                     && ur.IsActive != false
                     && r.IsDelete != true
                     && r.IsActive != false
                     && rp.IsDelete != true
                     && rp.IsGranted == true
                     && !string.IsNullOrWhiteSpace(rp.PermissionCode)
                 select new { rp.PermissionCode, rp.PermissionValue })
                .ToListAsync();

            var permissions = rawPermissions
                .Select(x => !string.IsNullOrWhiteSpace(x.PermissionValue)
                    ? $"{x.PermissionCode!.Trim()}.{x.PermissionValue.Trim()}"
                    : x.PermissionCode!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase) // OrdinalIgnoreCase giúp xác định không phân biệt hoa thường
                .ToList();

            if (cacheKey != null && !_cacheHelpers.IsCacheDisabled)
            {
                try
                {
                    await _cache.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(30));
                }
                catch
                {
                    _cacheHelpers.DisableCache();
                }
            }

            return new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase); // OrdinalIgnoreCase giúp xác định không phân biệt hoa thường
        }

        public async Task<bool> HasPermission(Guid userId, string permission)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(permission))
                return false;

            var permissions = await GetUserPermissions(userId);
            return permissions.Contains(permission);
        }

        public Task InvalidatePermissionCache()
        {
            return _cacheHelpers.InvalidateCacheAsync(VersionKey);
        }

        #endregion
    }
}
