using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : BaseController<IEmployeeService, Employee, EmployeeModel>
    {
        #region Infrastructure

        public EmployeeController(IEmployeeService employeeService) : base(employeeService)
        {

        }

        #endregion

        #region Default Operations

        #endregion

        #region Custom Operations

        #endregion
    }
}
