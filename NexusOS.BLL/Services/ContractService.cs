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

        protected override IQueryable<ContractModel>? ProjectToModel(IQueryable<Contract> query)
        {
            return query.Select(s => new ContractModel
            {
                Id = s.Id,
                ContractCode = s.ContractCode,
                ContractName = s.ContractName,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                ContractStatus = s.ContractStatus,
                BaseSalary = s.BaseSalary,
                SalaryUnit = s.SalaryUnit,
                WorkingHoursPerWeek = s.WorkingHoursPerWeek,
                Otrate = s.Otrate,
                AllowanceAmount = s.AllowanceAmount,
                InsuranceContribution = s.InsuranceContribution,
                SignedDate = s.SignedDate,
                SignedBy = s.SignedBy,
                ApprovalStatus = s.ApprovalStatus,
                Note = s.Note,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,

                ContractTypeName = s.ContractType != null && s.ContractType.IsDelete != true
                    ? s.ContractType.ContractTypeName : null,
                EmployeeName = s.Employee != null && s.Employee.IsDelete != true
                    ? s.Employee.EmployeeName : null
            });
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
