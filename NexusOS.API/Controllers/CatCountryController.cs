using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatCountryController : BaseController<ICatCountryService, CatCountry, CatCountryModel>
    {
        #region Infrastructure

        public CatCountryController(ICatCountryService appointmentService) : base(appointmentService)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
