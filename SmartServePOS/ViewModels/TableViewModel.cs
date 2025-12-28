using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SmartServePOS.ViewModels
{
	public class TableViewModel : INotifyPropertyChanged
	{
		private readonly IPrintService _printService;
		private readonly IBillingService _billingService;

		public ObservableCollection<GetTableViewDto> Tables { get; } = new();
		private readonly INavigationService _navigationService;
		public ICommand OpenTableCommand { get; }
		public ICommand PrintCommand { get; }
		private readonly IRestaurantTableStore _tableStore;
		public TableViewModel(
			IRestaurantTableStore tableStore, 
			INavigationService navigationService,
			IPrintService printService,
			IBillingService billingService)
		{
			_billingService = billingService;
			_printService = printService;
			_tableStore = tableStore ?? throw new ArgumentNullException(nameof(tableStore));
			_navigationService = navigationService;
			OpenTableCommand = new RelayCommand<GetTableViewDto>(OpenTable);
			PrintCommand = new RelayCommand<GetTableViewDto>(PrintBill);
			_ = InitializeAsync();
		}

		private async Task InitializeAsync()
		{
			try
			{
				var dtos = await _tableStore.GetTablesForViewAsync();
				Tables.Clear();
				foreach (var d in dtos)
				{
					Tables.Add(new GetTableViewDto
					{
						TableId = d.TableId,
						DisplayName = d.DisplayName ?? string.Empty,
						OrderId = d.OrderId,
						StatusName = d.StatusName,
						StatusCode = d.StatusCode,
						ColorHex = d.ColorHex,
						Amount = d.Amount,
						//IsOccupied = d.OrderId != null
					});
				}
				Notify(nameof(Tables));
			}
			catch (Exception)
			{
				// swallow or log as appropriate; leave sample fallback if desired.
			}
		}

		private async void OpenTable(GetTableViewDto table)
		{
			if (table == null)
				return;

			_navigationService.NavigateToBilling(table.OrderId??0,table.TableId);
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void Notify([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private async void PrintBill(GetTableViewDto bill)
		{
			if (bill != null && bill.OrderId != null && bill.OrderId >0)
			{
				var order = await _billingService.GetOrderAsync(Convert.ToInt32(bill.OrderId));
				if (order == null)
					return;
				var sumItems = order.OrderItems.Sum(x => x.Quantity * x.PriceSnapshot);
				BillPrintModel printbill = new BillPrintModel
				{
					ShopName = "Scoop Ice Cream Cafe",
					Address = "Sco 8, Basement, Fortune City Center\nSec. 123, Mohali-140301",

					//BillNo = _currentOrder.OrderNumber,
					//TableName = _currentOrder.TableName ?? "N/A",
					//Cashier = _currentUser?.Name ?? "biller",

					PrintedAt = DateTime.Now,

					Items = order.OrderItems.Select(x => new BillPrintItem
					{
						Name = x.Variant.Name,
						Quantity = x.Quantity,
						UnitPrice = x.PriceSnapshot
					}).ToList(),

					SubTotal = sumItems,
					//Discount = AppliedDiscountAmount,          // 0 if none
					//DiscountLabel = AppliedDiscountLabel,       // "10%" or ""
					GrandTotal = sumItems
				};

				_printService.PrintBill(printbill, showPreview: true);

				var billinbSaveRequest = new BillingSaveRequest
				{
					OrderId = bill.OrderId, 
					
					//OrderType = "DINE_IN",
					//TotalAmount = sumItems,
					//Items = order.OrderItems.Select(x => new BillingItem
					//{
					//	VariantId = x.VariantId??0,
					//	Quantity = x.Quantity,
					//	PriceSnapshot = x.PriceSnapshot
					//}).ToList()
				};
				var orderId = await _billingService.SaveOrderAsync(billinbSaveRequest);

			}
		}
		
	}

}
