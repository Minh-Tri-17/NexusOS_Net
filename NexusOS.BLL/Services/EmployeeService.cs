using Microsoft.EntityFrameworkCore;
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

        public override async Task<APIResults<PagingResults<EmployeeModel>>> GetPaging(FilterModel filter)
        {
            var result = await base.GetPaging(filter);
            if (result.IsSuccess && result.Result != null)
            {
                #region Enrich Related Data

                var countryIds = result.Result.Items.Select(s => s.CountryId)
                    .Where(s => s.HasValue).Distinct().ToList();

                var countries = await _context.CatCountries.AsNoTracking()
                    .Where(s => s.IsDelete != true && countryIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.CountryName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var provinceIds = result.Result.Items.Select(s => s.ProvinceId)
                    .Where(s => s.HasValue).Distinct().ToList();

                var provinces = await _context.CatProvinces.AsNoTracking()
                    .Where(s => s.IsDelete != true && provinceIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.ProvinceName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var wardIds = result.Result.Items.Select(s => s.WardId)
                    .Where(s => s.HasValue).Distinct().ToList();

                var wards = await _context.CatWards.AsNoTracking()
                    .Where(s => s.IsDelete != true && wardIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.WardName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var departmentIds = result.Result.Items.Select(s => s.DepartmentId)
                    .Where(s => s.HasValue).Distinct().ToList();

                var departments = await _context.CatDepartments.AsNoTracking()
                    .Where(s => s.IsDelete != true && departmentIds.Contains(s.Id))
                    .Select(s => new { s.Id, s.DepartmentName, s.ManagerId })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var employees = await _context.Employees.AsNoTracking()
                    .Where(s => s.IsDelete != true)
                    .Select(s => new { s.Id, s.EmployeeName })
                    .ToDictionaryAsync(s => s.Id, s => s);

                var employeeIds = result.Result.Items.Select(s => s.Id).ToList();

                var contracts = await _context.Contracts.AsNoTracking()
                    .Where(s => s.IsDelete != true && s.ContractTypeId.HasValue && s.EmployeeId.HasValue && employeeIds.Contains(s.EmployeeId.Value))
                    .Select(s => new { s.Id, s.EmployeeId, s.ContractName, s.ContractType!.ContractTypeName })
                    .ToDictionaryAsync(s => s.EmployeeId!.Value, s => s);

                foreach (var model in result.Result.Items)
                {
                    if (countries.TryGetValue(DataHelpers.GetGuid(model.CountryId), out var country))
                        model.CountryName = country.CountryName;

                    if (provinces.TryGetValue(DataHelpers.GetGuid(model.ProvinceId), out var province))
                        model.ProvinceName = province.ProvinceName;

                    if (wards.TryGetValue(DataHelpers.GetGuid(model.WardId), out var ward))
                        model.WardName = ward.WardName;

                    if (departments.TryGetValue(DataHelpers.GetGuid(model.DepartmentId), out var department))
                    {
                        model.DepartmentName = department.DepartmentName;

                        var manager = employees.TryGetValue(DataHelpers.GetGuid(department.ManagerId), out var managerInfo) ? managerInfo : null;
                        model.ManagerName = manager?.EmployeeName;
                    }

                    if (contracts.TryGetValue(DataHelpers.GetGuid(model.Id), out var contract))
                    {
                        model.ContractName = contract.ContractName;
                        model.ContractTypeName = contract.ContractTypeName;
                    }
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
