using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class EmployeeService : BaseService<Employee, EmployeeModel>, IEmployeeService
    {
        #region Infrastructure

        public EmployeeService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        protected override IQueryable<EmployeeModel> ProjectToModel(IQueryable<Employee> query)
        {
            return query.Select(s => new EmployeeModel
            {
                Id = s.Id,
                EmployeeCode = s.EmployeeCode,
                EmployeeName = s.EmployeeName,
                Gender = s.Gender,
                DateOfBirth = s.DateOfBirth,
                Address = s.Address,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                IdentityNumber = s.IdentityNumber,
                HireDate = s.HireDate,
                TerminationDate = s.TerminationDate,
                EmploymentStatus = s.EmploymentStatus,
                IsMultiStore = s.IsMultiStore,
                Photo = s.Photo,
                HealthInsuranceNumber = s.HealthInsuranceNumber,
                SocialInsuranceNumber = s.SocialInsuranceNumber,
                BankAccountNumber = s.BankAccountNumber,
                BankName = s.BankName,
                EducationLevel = s.EducationLevel,
                TaxCode = s.TaxCode,
                EmergencyContactName = s.EmergencyContactName,
                EmergencyContactPhone = s.EmergencyContactPhone,
                Note = s.Note,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,

                CountryName = s.Country != null && s.Country.IsDelete != true
                    ? s.Country.CountryName : null,
                ProvinceName = s.Province != null && s.Province.IsDelete != true
                    ? s.Province.ProvinceName : null,
                WardName = s.Ward != null && s.Ward.IsDelete != true
                    ? s.Ward.WardName : null,
                JobTitleName = s.JobTitle != null && s.JobTitle.IsDelete != true
                    ? s.JobTitle.JobTitleName : null,
                DepartmentName = s.Department != null && s.Department.IsDelete != true
                    ? s.Department.DepartmentName : null,
                ManagerName = s.Department != null && s.Department.Manager != null && s.Department.Manager.IsDelete != true
                    ? s.Department.Manager.EmployeeName : null,
                ContractName = s.Contracts
                    .Where(c => c.IsDelete != true && c.ContractTypeId != null)
                    .OrderByDescending(c => c.StartDate ?? c.CreatedAt)
                    .Select(c => c.ContractName)
                    .FirstOrDefault(),
                ContractTypeName = s.Contracts
                    .Where(c => c.IsDelete != true && c.ContractTypeId != null)
                    .OrderByDescending(c => c.StartDate ?? c.CreatedAt)
                    .Select(c => c.ContractType!.ContractTypeName)
                    .FirstOrDefault()
            });
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
