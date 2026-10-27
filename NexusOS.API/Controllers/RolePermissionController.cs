using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

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

        #endregion
    }
}
