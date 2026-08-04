using Microsoft.AspNetCore.Identity;
using NexusOS.MB;

namespace NexusOS.BLL.Interfaces
{
    public interface IAccountService
    {
        public Task<APIResults<string>> Auth(UserModel request);
        public Task<APIResults<bool>> SendOTP(MailModel mail);
        public Task<APIResults<bool>> ResetPass(UserModel request);
        public List<IdentityError> ValidatePass(string password);
    }
}
