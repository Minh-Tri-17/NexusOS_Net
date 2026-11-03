using Microsoft.AspNetCore.Mvc;
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

        public RolePermissionController(IRolePermissionService RolePermissionService) : base(RolePermissionService)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        [HttpGet($"{nameof(GetPermission)}")]
        public virtual async Task<ActionResult> GetPermission()
        {
            var result = typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .ToList();

            return Ok(result);
        }

        #endregion
    }
}
