using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : BaseController<IUserRoleService, UserRole, UserRoleModel>
    {
        #region Infrastructure

        public UserRoleController(IUserRoleService UserRoleService) : base(UserRoleService)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
