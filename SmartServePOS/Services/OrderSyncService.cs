using AutoMapper;
using SmartServe.Domain.Interfaces;
using SmartServe.Domain.Models;
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
					var payments = paymentLocal.Select(
						p => new Payment
						{
							OrderId = serverOrderId,
							Mode = p.Mode.ToString().ToUpper(),
							Amount = p.Amount,
							Status = p.Status,
							PartPaymentCash = p.PartPaymentCash,
							CreatedAt = ToUtc(p.CreatedAt)
						}
						).ToList();
					await _serverRepo.CreatePaymentsAsync(_mapper.Map<List<Payment>>(payments));
					await _localRepo.MarkOrderAsSyncedAsync(order.Id);
				}
			}
			catch
			{
				// log and continue
			}
		}


		#region private methods
		private static DateTime ToUtc(DateTime dt)
		{
			return dt.Kind switch
			{
				DateTimeKind.Utc => dt,
				DateTimeKind.Local => dt.ToUniversalTime(),
				DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
				_ => dt
			};
		}
		#endregion

	}

}
