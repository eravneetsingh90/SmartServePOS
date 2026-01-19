using HandyControl.Controls;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using SmartServePOS.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class TableViewModel : BaseViewModel
	{
		private readonly IPrintService _printService;
		private readonly IBillingService _billingService;
		private readonly ICatalogService _catalogService;
		public bool IsPaymentPopupOpen { get; set; }
		public int SelectedTableId { get; set; }
		public int SelectedOrderId { get; set; }
		public decimal BillTotal { get; set; }
		public string SelectedTableName { get; set; }

		public ObservableCollection<GetTableViewDto> Tables { get; } = new();
		private readonly INavigationService _navigationService;
		public ICommand OpenTableCommand { get; }
		public ICommand PrintCommand { get; }
		public ICommand ClosePaymentPopupCommand { get; }
		public ICommand OpenPaymentCommand { get; }
		public ICommand SettleAndSaveCommand { get; }
		private readonly IRestaurantTableStore _tableStore;
		public bool IsPartPayment => SelectedPaymentMode == Helper.PaymentMode.Part;
		private decimal _partPaymentCash;
		public decimal PartPaymentCash
		{
			get => _partPaymentCash;
			set
			{
				_partPaymentCash = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RemainingAmount));
			}
		}
		public decimal RemainingAmount =>
	Math.Max(0, BillTotal - PartPaymentCash);
		private Helper.PaymentMode _selectedPaymentMode;
		public Helper.PaymentMode SelectedPaymentMode
		{
			get => _selectedPaymentMode;
			set
			{
				_selectedPaymentMode = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsPartPayment));
				OnPropertyChanged(nameof(RemainingAmount));
			}
		}
		public TableViewModel(
			IRestaurantTableStore tableStore,
			INavigationService navigationService,
			IPrintService printService,
			IBillingService billingService,
			ICatalogService catalogService)
		{
			_catalogService = catalogService;
			_billingService = billingService;
			_printService = printService;
			_tableStore = tableStore ?? throw new ArgumentNullException(nameof(tableStore));
			_navigationService = navigationService;
			OpenTableCommand = new RelayCommand<GetTableViewDto>(OpenTable);
			PrintCommand = new RelayCommand<GetTableViewDto>(PrintBill);
			//SaveCommand = new RelayCommand<GetTableViewDto>(SaveAsync);
			OpenPaymentCommand = new RelayCommand<GetTableViewDto>(OpenPayment);
			SettleAndSaveCommand = new RelayCommand<GetTableViewDto>(SettleAndSaveAsync);
			ClosePaymentPopupCommand = new RelayCommand(_ => ClosePaymentPopup());
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

			_navigationService.NavigateToBillingView(table.OrderId ?? 0, table.TableId);
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void Notify([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private async void PrintBill(GetTableViewDto bill)
		{
			if (bill != null && bill.OrderId != null && bill.OrderId > 0)
			{
				int orderId = Convert.ToInt32(bill.OrderId);
				var order = await _billingService.GetOrderAsync(orderId);
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
						Name = x.Variant?.VariantName??string.Empty,
						Quantity = x.Quantity,
						UnitPrice = x.PriceSnapshot
					}).ToList(),

					SubTotal = sumItems,
					//Discount = AppliedDiscountAmount,          // 0 if none
					//DiscountLabel = AppliedDiscountLabel,       // "10%" or ""
					GrandTotal = sumItems
				};

				_printService.PrintBill(printbill, showPreview: false);
				order.StatusId = _catalogService.GetTableStatusByCode(TableStatusCodes.PRINTED).Id;
				await _billingService.UpdateOrderAsync(order);
				_ = InitializeAsync();

			}
		}

		//private async void SaveAsync(GetTableViewDto bill)
		//{
		//	if (bill != null && bill.OrderId != null && bill.OrderId > 0)
		//	{
		//		int orderId = Convert.ToInt32(bill.OrderId);
		//		var order = await _billingService.GetOrderAsync(orderId);
		//		if (order == null)
		//			return;
		//		order.StatusId = _catalogService.GetTableStatusByCode(TableStatusCodes.BLANK).StatusId;
		//		order.ClosedAt = DateTime.UtcNow;
		//		await _billingService.UpdateOrderAsync(order);
		//		_ = InitializeAsync();

		//	}
		//}
		private void OpenPayment(GetTableViewDto table)
		{
			if (table.OrderId == null)
				return;

			SelectedTableId = table.TableId;
			SelectedOrderId = table.OrderId.Value;
			SelectedTableName = table.DisplayName;
			BillTotal = table.Amount;

			IsPaymentPopupOpen = true;

			OnPropertyChanged(nameof(IsPaymentPopupOpen));
			OnPropertyChanged(nameof(SelectedTableName));
			OnPropertyChanged(nameof(BillTotal));
		}
		private async void SettleAndSaveAsync(GetTableViewDto sd)
		{
			IsPaymentPopupOpen = false;
			OnPropertyChanged(nameof(IsPaymentPopupOpen));


			await _billingService.CloseOrderAsync(
				SelectedOrderId,
				new Payment
				{
					OrderId=SelectedOrderId,
					Mode = SelectedPaymentMode.ToString().ToUpper(),
					Amount = BillTotal,
					PartPaymentCash = PartPaymentCash
				});
			_ = InitializeAsync();
		}
		private void ClosePaymentPopup()
		{
			IsPaymentPopupOpen = false;
			OnPropertyChanged(nameof(IsPaymentPopupOpen));
		}
	}

}
