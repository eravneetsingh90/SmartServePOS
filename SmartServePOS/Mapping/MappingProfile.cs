using AutoMapper;
using SmartServe.Domain.Models;
using SmartServePOS.Models;

namespace SmartServePOS.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<BrandDto, BrandModel>().ReverseMap();
			CreateMap<AddStock, AddStockModel>().ReverseMap();
			CreateMap<Product, ProductModel>().ReverseMap();
		}
	}
}
