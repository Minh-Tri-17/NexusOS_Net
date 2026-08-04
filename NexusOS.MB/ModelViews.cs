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

    public class FilterModel
    {
        public bool AllowPaging { get; set; } = true;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
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

    }

    public partial class CatWardModel : CatWard
    {

    }

    public partial class UserModel : User
    {
        public bool Remember { get; set; } = false;
        [Required]
        public string? Password { get; set; }
        public string? RoleIds { get; set; }
        public string? Otp { get; set; }
    }

    #endregion
}
