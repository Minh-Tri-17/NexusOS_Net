using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseController<IRoleService, Role, RoleModel>
    {
        #region Infrastructure

        public RoleController(IRoleService RoleService) : base(RoleService)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
