using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class CatWardService : BaseService<CatWard, CatWardModel>, ICatWardService
    {
        #region Infrastructure

        public CatWardService(NexusOsContext context, ICurrentUserService currentUser,
            IStringLocalizer<SharedResource> localizer) : base(context, currentUser, localizer)
        {

        }

        #endregion

        #region Default Operations

        protected override IQueryable<CatWardModel>? ProjectToModel(IQueryable<CatWard> query)
        {
            return query.Select(s => new CatWardModel
            {
                Id = s.Id,
                WardCode = s.WardCode,
                WardName = s.WardName,
                Note = s.Note,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,

                ProvinceName = s.Province != null && s.Province.IsDelete != true
                    ? s.Province.ProvinceName : null
            });
        }

        #endregion

        #region Custom Operations

        #endregion
    }
}
