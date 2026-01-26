using SmartServe.Domain.Services;
using SmartServePOS.Models;
using System.Collections.ObjectModel;

namespace SmartServePOS.ViewModels
{
	public class OrderDetailsViewModel : BaseViewModel
	{
		private readonly IOrderReportService _reportService;

		public OrderDetailsViewModel(IOrderReportService reportService)
		{
			_reportService = reportService;
		}

		public ObservableCollection<OrderItemDto> Items { get; }
			= new();

		public async Task LoadAsync(int orderId)
		{
			var result = await _reportService.GetOrderItemsAsync(orderId);

			Items.Clear();
			foreach (var item in result)
				Items.Add(new OrderItemDto
				{
					Id = item.Id,
					OrderId = item.OrderId ?? 0,
					Quantity = item.Quantity,
					PriceSnapshot = item.PriceSnapshot,
					DiscountAmount = item.DiscountAmount ?? 0,
					ProductName = item.Variant?.Product?.Name,
					VariantName = item.Variant?.VariantName
				});
		}
	}

}
