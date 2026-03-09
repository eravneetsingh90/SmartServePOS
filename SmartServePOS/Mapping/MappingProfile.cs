using AutoMapper;
using SmartServe.Common.Models;
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
			CreateMap<CurrentStock, CurrentStockDto>().ReverseMap();
			CreateMap<OrderItem, OrderItemDto>().ReverseMap();
			CreateMap<Payment, PaymentDto>().ReverseMap();
			CreateMap<Stock, LinkInventoryDto>().ReverseMap();
			CreateMap<AddStock, AddStockModel>().ReverseMap();
			CreateMap<Product, ProductDto>().ReverseMap();
			CreateMap<Category, CategoryDto>().ReverseMap();
			CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();
			CreateMap <RestaurantTable, RestaurantTableDto>().ReverseMap();
			CreateMap<TableStatusEntity, TableStatusDto>().ReverseMap();
		}
	}
}
