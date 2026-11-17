using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.API.Filters
{
    public class ConventionPermissionFilter : IAsyncAuthorizationFilter
    {
        private static readonly IReadOnlyDictionary<string, string>
            ActionPermissionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["GetOne"] = "View",
                ["GetPaging"] = "View",
                ["Create"] = "Create",
                ["Update"] = "Update",
                ["SoftDelete"] = "Delete",
                ["HardDelete"] = "Delete",
                ["Import"] = "Import",
                ["Export"] = "Export"
            };

        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permissionService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ConventionPermissionFilter(ICurrentUserService currentUser, IPermissionService permissionService,
            IStringLocalizer<SharedResource> localizer)
        {
            _currentUser = currentUser;
            _permissionService = permissionService;
            _localizer = localizer;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var endpointMetadata = context.ActionDescriptor.EndpointMetadata;
            var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated;
            var userId = _currentUser.UserId;

            // Bỏ qua kiểm tra quyền nếu endpoint được đánh dấu công khai ([AllowAnonymous]).
            if (endpointMetadata.OfType<IAllowAnonymous>().Any())
                return;

            // Chặn truy cập (trả về 401) nếu người dùng chưa đăng nhập hoặc UserId không hợp lệ.
            if (isAuthenticated != true || userId == Guid.Empty)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var requiredPermission = ResolveRequiredPermission(context);
            if (string.IsNullOrWhiteSpace(requiredPermission))
                return;

            var hasPermission = await _permissionService.HasPermission(userId, requiredPermission);

            // Kiểm tra quyền của người dùng trả về lỗi 403 (Forbidden) nếu không đủ quyền.
            if (!hasPermission)
            {
                context.Result = new ObjectResult(APIResults<object>.Failure(_localizer[Messages.Forbidden]))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }

        private static string? ResolveRequiredPermission(AuthorizationFilterContext context)
        {
            var explicitPermission = context.ActionDescriptor.EndpointMetadata
                .OfType<HasPermissionAttribute>()
                .LastOrDefault()?.Permission;

            // Trả về ngay quyền được cấu hình tường minh (nếu có).
            if (!string.IsNullOrWhiteSpace(explicitPermission))
                return explicitPermission;

            // Loại bỏ các endpoint không phải là Controller API
            if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
                return null;

            // Bỏ qua kiểm tra quyền nếu Action không nằm trong danh sách phân quyền động.
            if (!ActionPermissionMap.TryGetValue(descriptor.ActionName, out var actionSuffix))
                return null;

            return $"{descriptor.ControllerName}.{actionSuffix}";
        }
    }
}
