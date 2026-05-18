using api_server.Model;
using api_server.Models;
using AutoMapper;
using server_API.DTO;

namespace server_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== Gift -> GiftDTO =====
            CreateMap<Gift, GiftDTO>()
                .ForMember(dest => dest.donorName,
                           opt => opt.MapFrom(src => src.Donor.Name))
                .ForMember(dest => dest.DonorId,
                           opt => opt.MapFrom(src => src.Donor.Id))
                .ForMember(dest => dest.Category,
                           opt => opt.MapFrom(src => src.category.ToString()))
                .ForMember(dest => dest.PurchasersCount,
                           opt => opt.MapFrom(src => src.Purchases.Count));


            // ===== GiftDTO -> Gift =====
            CreateMap<GiftDTO, Gift>()
                .ForMember(dest => dest.category,
                           opt => opt.MapFrom(src => ParseCategory(src.Category)))
                .ForMember(dest => dest.Donor,
                           opt => opt.Ignore());

            // ===== Donor mappings =====
            CreateMap<DonorDTO, Donor>()
                .ForMember(dest => dest.GiftList,
                           opt => opt.Ignore());

            CreateMap<Donor, DonorDTO>();
        }

        private static Category ParseCategory(string str)
        {
            if (Enum.TryParse<Category>(str, true, out var result))
                return result;

            return Category.All_prizes;
        }
    }
}
