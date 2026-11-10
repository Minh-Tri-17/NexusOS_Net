using Microsoft.AspNetCore.Http;
using NexusOS.DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace NexusOS.MB
{
    #region Model base

    public class MailModel
    {
        [Required]
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<string> CC { get; set; } = new List<string>();
        public List<string> BCC { get; set; } = new List<string>();
        public List<IFormFile>? Attachments { get; set; }
    }

    public class OTPModel
    {
        public string? Otp { get; set; }
        public string? Email { get; set; }
    }

    public class OtpEntry
    {
        public string Hashed { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int AttemptsLeft { get; set; } = 3;
    }

    public class FilterModel
    {
        public bool AllowPaging { get; set; } = true;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool IsExport { get; set; } = false;
        public Guid? IdMain { get; set; }
        public List<FilterItemModel> Filters { get; set; } = new List<FilterItemModel>();
    }

    public class FilterItemModel
    {
        public string? FilterName { get; set; }
        public string? FilterValue { get; set; }
        public string? FilterType { get; set; }
        public string? FilterOperator { get; set; }
    }

    #endregion

    #region Model database

    public partial class CatCountryModel : CatCountry
    {

    }

    public partial class CatProvinceModel : CatProvince
    {
        public string? CountryName { get; set; }
    }

    public partial class CatWardModel : CatWard
    {
        public string? ProvinceName { get; set; }
    }

    public partial class CatDepartmentModel : CatDepartment
    {
        public string? ManagerName { get; set; }
    }

    public partial class CatLeaveTypeModel : CatLeaveType
    {

    }

    public partial class CatContractTypeModel : CatContractType
    {

    }

    public partial class CatJobTitleModel : CatJobTitle
    {

    }

    public partial class EmployeeModel : Employee
    {
        public string? CountryName { get; set; }
        public string? ProvinceName { get; set; }
        public string? WardName { get; set; }
        public string? JobTitleName { get; set; }
        public string? DepartmentName { get; set; }
        public string? ContractName { get; set; }
        public string? ContractTypeName { get; set; }
        public string? ManagerName { get; set; }
    }

    public partial class LeaveRequestModel : LeaveRequest
    {

    }

    public partial class ContractModel : Contract
    {
        public string? ContractTypeName { get; set; }
        public string? EmployeeName { get; set; }
    }

    public partial class UserModel : User
    {
        public bool Remember { get; set; } = false;
        public string? Password { get; set; }
        public List<string>? RoleIds { get; set; }
        public string? EmployeeName { get; set; }
    }

    public partial class RoleModel : Role
    {
        public List<RolePermission>? Permissions { get; set; }
    }

    public partial class UserRoleModel : UserRole
    {

    }

    public partial class RolePermissionModel : RolePermission
    {

    }

    #endregion
}
