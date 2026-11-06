using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class ContractService : BaseService<Contract, ContractModel>, IContractService
    {
        #region Infrastructure

        public ContractService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        public override async Task<APIResults<PagingResults<ContractModel>>> GetPaging(FilterModel filter)
        {
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                #region Enrich Related Data

                var contractTypeIds = result.Result.Items.Select(s => s.ContractTypeId)
                   .Where(s => s.HasValue).Distinct().ToList();

                var contracts = await _context.CatContractTypes.AsNoTracking()
                    .Where(s => s.IsDelete != true && contractTypeIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.ContractTypeName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var employeeIds = result.Result.Items.Select(s => s.EmployeeId)
                   .Where(s => s.HasValue).Distinct().ToList();

                var employees = await _context.Employees.AsNoTracking()
                    .Where(s => s.IsDelete != true && employeeIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.EmployeeName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                foreach (var model in result.Result.Items)
                {
                    if (contracts.TryGetValue(DataHelpers.GetGuid(model.ContractTypeId), out var contractType))
                        model.ContractTypeName = contractType.ContractTypeName;

                    if (employees.TryGetValue(DataHelpers.GetGuid(model.EmployeeId), out var employee))
                        model.EmployeeName = employee.EmployeeName;
                }

                #endregion
            }

            return result;
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
