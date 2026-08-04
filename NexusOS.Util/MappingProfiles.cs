using AutoMapper;
using NexusOS.DAL.Models;
using NexusOS.MB;

namespace NexusOS.Util
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Khi cập nhật (map từ model → entity nhưng bỏ 4 field)
            CreateMap<CatCountry, CatCountryModel>().ReverseMap()
                .ForMember(dest => dest.CreatedBy, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.CountryCode, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.IsDelete, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true));

            // Khi cập nhật (map từ model → entity nhưng bỏ 4 field)
            CreateMap<CatProvince, CatProvinceModel>().ReverseMap()
                .ForMember(dest => dest.CreatedBy, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.ProvinceCode, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.IsDelete, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true));

            // Khi cập nhật (map từ model → entity nhưng bỏ 4 field)
            CreateMap<CatWard, CatWardModel>().ReverseMap()
                .ForMember(dest => dest.CreatedBy, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.WardCode, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.IsDelete, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true));

            // Khi cập nhật (map từ model → entity nhưng bỏ 4 field)
            CreateMap<User, UserModel>().ReverseMap()
                .ForMember(dest => dest.CreatedBy, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.UserCode, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true))
                .ForMember(dest => dest.IsDelete, opt => opt.Condition((src, dest, srcMember, destMember, context) => !context.Items.TryGetValue("IgnoreAuditFields", out var ignore) || ignore is not true));
        }
    }
}
