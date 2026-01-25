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
			CreateMap<Order, OrderDto>().ReverseMap();
			CreateMap<AddStock, AddStockModel>().ReverseMap();
			CreateMap<Product, ProductDto>().ReverseMap();
			CreateMap<Category, CategoryDto>().ReverseMap();
			CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();
			CreateMap <RestaurantTable, RestaurantTableDto>().ReverseMap();
			CreateMap<TableStatusEntity, TableStatusDto>().ReverseMap();
		}
	}
}
