using AutoMapper;
using SmartServe.EFCore.Models;
using SmartServePOS.Models;

namespace SmartServePOS.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<ProductVariant, ProductVariantModel>();
			CreateMap<ProductVariantModel, ProductVariant>();
			CreateMap<Brand, BrandModel>().ReverseMap();

		}
	}
}
