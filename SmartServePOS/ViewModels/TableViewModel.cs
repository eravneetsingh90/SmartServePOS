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
using System.Windows;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class TableViewModel : BaseViewModel
	{
		private readonly IPOSBillingService _billingService;
		private readonly IPrintService _printService;
		private readonly IPOSCatalogService _catalogService;
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
			IPOSBillingService billingService,
			INavigationService navigationService,
			IPrintService printService,
			IPOSCatalogService catalogService)
		{
			_catalogService = catalogService;
			_billingService = billingService;
			_printService = printService;
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
				var dtos = await _billingService.GetTablesForViewAsync();
				Tables.Clear();
				foreach (var d in dtos)
				{
					Tables.Add(d);
				}
				Notify(nameof(Tables));
			}
			catch (Exception)
			{
				// swallow or log as appropriate; leave sample fallback if desired.
			}
		}

		//private async void OpenTable(GetTableViewDto table)
		//{
		//	if (table == null)
		//		return;

		//	_navigationService.NavigateToBillingView(table.OrderId ?? 0, table.TableId);
		//}
		private void OpenTable(GetTableViewDto table)
		{
			if (table == null)
				return;

			// IMPORTANT: defer navigation
			Application.Current.Dispatcher.BeginInvoke(
				new Action(async () =>
				{
					await _navigationService.NavigateToBillingView(
						table.OrderId ?? 0,
						table.TableId
					);
				}),
				System.Windows.Threading.DispatcherPriority.Background
			);
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
				var subTotal = order.OrderItems.Sum(x => x.Quantity * x.PriceSnapshot);
				decimal discount = 0;
				if (order.DiscountType == "PERCENT")
				{
					discount = Math.Round(subTotal * order.DiscountValue / 100, 2);
				}

				if (order.DiscountType == "FLAT")
				{
					discount = order.DiscountValue;
				}
				BillPrintModel printbill = new BillPrintModel
				{
					ShopName = "Scoop Ice Cream Cafe",
					Address = "Sco 8, Basement, Fortune City Center\nSec. 123, S.A.S Nagar-140301",

					BillNo = order.OrderNumber,
					//TableName = _currentOrder.TableName ?? "N/A",
					Cashier = "biller",

					PrintedAt = DateTime.Now,

					Items = order.OrderItems.Select(x => new BillPrintItem
					{
						Name = x.Variant.Product.Name ??string.Empty,
						VariantName = x.Variant?.VariantName ?? string.Empty,
						Quantity = x.Quantity,
						UnitPrice = x.PriceSnapshot
					}).ToList(),

					SubTotal = subTotal,
					Discount = discount,          // 0 if none
					//DiscountLabel = AppliedDiscountLabel,       // "10%" or ""
					GrandTotal = subTotal - discount
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
				new PaymentDto
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
