using AutoMapper;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public class OrderSyncService : IOrderSyncService
	{
		private readonly IMapper _mapper;
		private readonly IPOSBillingService _localRepo;
		private readonly IBillingService _serverRepo;

		public OrderSyncService(IMapper mapper, IPOSBillingService localRepo, IBillingService serverRepo)
		{
			_mapper = mapper;
			_localRepo = localRepo;
			_serverRepo = serverRepo;
		}

		public async Task SyncPendingOrdersAsync()
		{
			var orders = await _localRepo.GetUnsyncedOrdersAsync();

			foreach (var order in orders)
			{
				await SyncSingleOrderSafeAsync(order);
			}
		}

		private async Task SyncSingleOrderSafeAsync(OrderDto order)
		{
			try
			{
				var localOrder = _mapper.Map<Order>(order);
				localOrder.Id = 0;
				var serverOrderId = await _serverRepo.CreateOrderAsync(localOrder);
				if (serverOrderId > 0)
				{
					var orderItemsLocal = await _localRepo.GetOrderItemsByOrderIdAsync(order.Id);
					var orderItems = orderItemsLocal.Select(x => new OrderItem
					{
						OrderId = serverOrderId,
						VariantId = x.VariantId,
						Quantity = x.Quantity,
						PriceSnapshot = x.PriceSnapshot
					}).ToList();

					await _serverRepo.CreateOrderItemsAsync(orderItems);

					var paymentLocal = await _localRepo.GetPaymentByOrderIdAsync(order.Id);
					await _serverRepo.CloseOrderAsync(
					serverOrderId,
					new Payment
					{
						OrderId = serverOrderId,
						Mode = paymentLocal.Mode.ToString().ToUpper(),
						Amount = paymentLocal.Amount,
						Status = paymentLocal.Status,
						PartPaymentCash = paymentLocal.PartPaymentCash
					});
					await _localRepo.MarkOrderAsSyncedAsync(order.Id);
				}
			}
			catch
			{
				// log and continue
			}
		}
		
	}

}
