using AutoMapper;
using SmartServe.Domain.Models;
using SmartServe.EFCore.Models;
using SmartServePOS.Models;

namespace SmartServePOS.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Brand, BrandModel>().ReverseMap();
			CreateMap<CategoryDto, CategoryModel>().ReverseMap();
			CreateMap<ProductDto, ProductModel>().ReverseMap();
			CreateMap<ProductVariantDto, ProductVariantModel>().ReverseMap();
		}
	}
}
