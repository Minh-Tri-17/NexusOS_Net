using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController<IUserService, User, UserModel>
    {
        #region Infrastructure

        public UserController(IUserService UserService) : base(UserService)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
