using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;
using System.Reflection;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : BaseController<IRolePermissionService, RolePermission, RolePermissionModel>
    {
        #region Infrastructure

        private readonly IStringLocalizer<SharedResource> _localizer;

        public RolePermissionController(
            IRolePermissionService RolePermissionService,
            IStringLocalizer<SharedResource> localizer) : base(RolePermissionService)
        {
            _localizer = localizer;
        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        [HttpGet($"{nameof(GetPermission)}")]
        public virtual ActionResult<APIResults<List<string>>> GetPermission()
        {
            var result = typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .ToList();

            return Ok(APIResults<List<string>>.Success(result, _localizer[Messages.GetListResultSuccess]));
        }

        #endregion
    }
}
