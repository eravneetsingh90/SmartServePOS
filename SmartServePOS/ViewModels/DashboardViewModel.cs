using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class DashboardViewModel : BaseViewModel
	{
		private readonly IMapper _mapper;
		private readonly IOrderReportService _reportService;
		private readonly INavigationService _navigationService;
		private readonly SyncScheduler _sync;
		public Array DateRanges => Enum.GetValues(typeof(DateRangeType));
		public DashboardViewModel(
			IMapper mapper,
			IOrderReportService reportService,
			INavigationService navigationService,
			SyncScheduler sync)
		{
			_sync = sync;
			_mapper = mapper;
			_reportService = reportService;
			_navigationService = navigationService;

			Orders = new ObservableCollection<OrderGridDto>();

			SelectedRange = DateRangeType.Today;
			FromDate = DateTime.Today;
			ToDate = DateTime.Today;

			LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
			SyncOrdersCommand = new AsyncRelayCommand(SyncOrdersAsync);
			OpenOrderDetailsCommand = new RelayCommand<OrderGridDto>(OpenOrderDetails);


		}

		public async Task InitializeAsync()
		{
			await LoadOrdersAsync();
		}
		// =====================
		// FILTER
		// =====================

		private DateRangeType _selectedRange;
		public DateRangeType SelectedRange
		{
			get => _selectedRange;
			set
			{
				if (_selectedRange == value)
					return;
				_selectedRange = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsCustomRangeVisible));
				// Auto-load for predefined ranges
				if (value != DateRangeType.Custom)
				{
					_ = LoadOrdersAsync();
				}
			}
		}

		public bool IsCustomRangeVisible =>
			SelectedRange == DateRangeType.Custom;

		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }

		// =====================
		// SUMMARY
		// =====================

		private decimal _totalSales;
		public decimal TotalSales
		{
			get => _totalSales;
			set { _totalSales = value; OnPropertyChanged(); }
		}

		private decimal _cashSales;
		public decimal CashSales
		{
			get => _cashSales;
			set { _cashSales = value; OnPropertyChanged(); }
		}

		private decimal _upiSales;
		public decimal UpiSales
		{
			get => _upiSales;
			set { _upiSales = value; OnPropertyChanged(); }
		}

		private int _totalOrders;
		public int TotalOrders
		{
			get => _totalOrders;
			set { _totalOrders = value; OnPropertyChanged(); }
		}

		// =====================
		// GRID
		// =====================

		public ObservableCollection<OrderGridDto> Orders { get; }

		private OrderGridDto _selectedOrder;
		public OrderGridDto SelectedOrder
		{
			get => _selectedOrder;
			set
			{
				_selectedOrder = value;
				OnPropertyChanged();
				if (value != null)
					OpenOrderDetailsCommand.Execute(value);
			}
		}

		// =====================
		// COMMANDS
		// =====================

		public ICommand LoadOrdersCommand { get; }
		public ICommand SyncOrdersCommand { get; }
		public ICommand OpenOrderDetailsCommand { get; }
		public ICommand ToggleExpandCommand => new RelayCommand<OrderGridDto>(ToggleExpand);


		// =====================
		// LOGIC
		// =====================
		private async void ToggleExpand(OrderGridDto order)
		{
			order.IsExpanded = !order.IsExpanded;

			if (order.IsExpanded && order.Items.Count == 0)
			{
				// lazy load details
				var details = await _reportService.GetOrderItemsAsync(order.Id);

				order.Items.Clear();
				foreach (var item in details)
					order.Items.Add(new OrderItemDto
					{
						Id = item.Id,
						OrderId = item.OrderId??0,
						Quantity = item.Quantity,
						PriceSnapshot = item.PriceSnapshot,
						DiscountAmount = item.DiscountAmount ?? 0,

						ProductName = item.Variant?.Product?.Name,
						VariantName = item.Variant?.VariantName
					});
			}
		}
		private async Task LoadOrdersAsync()
		{
			var (fromUtc, toUtc) = ResolveDateRangeUtc();

			var result = await _reportService.GetOrdersAsync(fromUtc, toUtc);

			Orders.Clear();

			foreach (var order in result.Orders)
			{
				Orders.Add(new OrderGridDto
				{
					Id = order.Id,
					OrderNumber = order.OrderNumber,
					OrderType = order.OrderType,
					TotalAmount = order.TotalAmount ?? 0,
					DisplayTime = ConvertToIST(order.CreatedAt ?? DateTime.MinValue),
					Discount = (order.DiscountType == DiscountType.PERCENT ? order.TotalAmount * order.DiscountValue / (100 - order.DiscountValue) : order.DiscountValue) ?? 0
				});
			}

			TotalSales = result.TotalSales;
			CashSales = result.CashSales;
			UpiSales = result.UpiSales;
			TotalOrders = result.TotalOrders;

		}

		private async Task SyncOrdersAsync()
		{
			await _sync.RunOnceSafeAsync();
			await LoadOrdersAsync();
		}

		private void OpenOrderDetails(OrderGridDto order)
		{
			_navigationService.OpenOrderDetailsDialog(order.Id);
			SelectedOrder = null; // reset selection
		}

		// =====================
		// DATE RANGE LOGIC
		// =====================

		private (DateTime fromUtc, DateTime toUtc) ResolveDateRangeUtc()
		{
			var now = DateTime.Now;

			DateTime fromLocal, toLocal;

			switch (SelectedRange)
			{
				case DateRangeType.Today:
					fromLocal = now.Date;
					toLocal = now.Date.AddDays(1);
					break;

				case DateRangeType.Yesterday:
					fromLocal = now.Date.AddDays(-1);
					toLocal = now.Date;
					break;

				case DateRangeType.Last7Days:
					fromLocal = now.Date.AddDays(-6);
					toLocal = now.Date.AddDays(1);
					break;

				case DateRangeType.ThisMonth:
					fromLocal = new DateTime(now.Year, now.Month, 1);
					toLocal = fromLocal.AddMonths(1);
					break;

				case DateRangeType.LastMonth:
					toLocal = new DateTime(now.Year, now.Month, 1);
					fromLocal = toLocal.AddMonths(-1);
					break;

				default:
					fromLocal = FromDate.Date;
					toLocal = ToDate.Date.AddDays(1);
					break;
			}

			return (
				ToUtc(fromLocal),
				ToUtc(toLocal)
			);
		}

		private static DateTime ToUtc(DateTime dateTime)
		{
			// Force Unspecified so IST can be applied safely
			var unspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

			return TimeZoneInfo.ConvertTimeToUtc(
				unspecified,
				TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
			);
		}

		private static string ConvertToIST(DateTime utc)
		{
			var ist = TimeZoneInfo.ConvertTimeFromUtc(
				DateTime.SpecifyKind(utc, DateTimeKind.Utc),
				TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
			);

			return ist.ToString("dd/MM/yyyy hh:mm tt");
		}
	}
}


