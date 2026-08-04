using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.BLL.Services
{
    public class CatCountryService : BaseService<CatCountry, CatCountryModel>, ICatCountryService
    {
        #region Infrastructure

        public CatCountryService(NexusOsContext context, ICurrentUserService currentUser, IStringLocalizer localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
