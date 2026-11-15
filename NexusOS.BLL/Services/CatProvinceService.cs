using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class CatProvinceService : BaseService<CatProvince, CatProvinceModel>, ICatProvinceService
    {
        #region Infrastructure

        public CatProvinceService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        protected override IQueryable<CatProvinceModel> ProjectToModel(IQueryable<CatProvince> query)
        {
            return query.Select(s => new CatProvinceModel
            {
                Id = s.Id,
                ProvinceCode = s.ProvinceCode,
                ProvinceName = s.ProvinceName,
                Note = s.Note,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,

                CountryName = s.Country != null && s.Country.IsDelete != true
                    ? s.Country.CountryName : null
            });
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
