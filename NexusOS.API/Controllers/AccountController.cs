using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        #region Infrastructure

        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        [HttpPost(nameof(Authentication))]
        public async Task<ActionResult<User>> Authentication([FromBody] UserModel user)
        {
            var result = await _accountService.Auth(user);
            return Ok(result);
        }

        [HttpPost(nameof(SendOTP))]
        public async Task<ActionResult<APIResults<bool>>> SendOTP([FromBody] MailModel mail)
        {
            var result = await _accountService.SendOTP(mail);
            return Ok(result);
        }

        [HttpPatch(nameof(ResetPassword))]
        public async Task<ActionResult<APIResults<bool>>> ResetPassword([FromBody] UserModel user)
        {
            var result = await _accountService.ResetPass(user);
            return Ok(result);
        }

        #endregion
    }
}
