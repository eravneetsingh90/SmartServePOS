using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public interface IPOSBillingService
	{
		Task<List<GetTableViewDto>> GetTablesForViewAsync();
		Task<OrderDto> GetOrderAsync(int orderId);
		Task<int> CreateOrderAsync(OrderDto request);
		Task UpdateOrderAsync(OrderDto dto);
		Task CreateOrderItemsAsync(List<OrderItemDto> orderItems);
		Task UpdateOrderItemsAsync(int orderId, List<OrderItemDto> items);
		Task CloseOrderAsync(int orderId, PaymentDto payment);
		Task<List<OrderDto>> GetUnsyncedOrdersAsync();
		Task<List<OrderItemDto>> GetOrderItemsByOrderIdAsync(int orderId);
		Task<List<PaymentDto>> GetPaymentByOrderIdAsync(int orderId);
		Task MarkOrderAsSyncedAsync(int id);
	}
}
