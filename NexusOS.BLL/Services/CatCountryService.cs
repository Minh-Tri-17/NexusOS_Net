using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class CatCountryService : BaseService<CatCountry, CatCountryModel>, ICatCountryService
    {
        #region Infrastructure

        public CatCountryService(NexusOsContext context, ICurrentUserService currentUser, IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
