using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using SmartServePOS.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class DashboardViewModel : BaseViewModel
	{
		private readonly IMapper _mapper;
		private readonly IOrderReportService _reportService;
		private readonly INavigationService _navigationService;
		
		public Array DateRanges => Enum.GetValues(typeof(DateRangeType));
		public DashboardViewModel(
			IMapper mapper,
			IOrderReportService reportService,
			INavigationService navigationService
			)
		{
			_mapper = mapper;
			_reportService = reportService;
			_navigationService = navigationService;

			Orders = new ObservableCollection<OrderGridDto>();

			SelectedRange = DateRangeType.Today;
			FromDate = DateTime.Today;
			ToDate = DateTime.Today;

			LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync);
			
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

		public OrderGridDto SelectedOrder { get; set; }

		// =====================
		// GRID
		// =====================

		public ObservableCollection<OrderGridDto> Orders { get; }

		// =====================
		// COMMANDS
		// =====================

		public ICommand LoadOrdersCommand { get; }
		
		public ICommand OpenOrderDetailsCommand { get; }


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
					Discount = (String.IsNullOrWhiteSpace(order.DiscountType) ? 0 : order.OriginalAmount-order.TotalAmount)??0
				});
			}

			TotalSales = result.TotalSales;
			CashSales = result.CashSales;
			UpiSales = result.UpiSales;
			TotalOrders = result.TotalOrders;

		}

		
		private async void OpenOrderDetails(OrderGridDto? dto)
		{
			if (SelectedOrder == null)
				return;

			var vm = App.Services.GetService<OrderDetailsViewModel>();

			await vm.LoadAsync(SelectedOrder.Id);

			var view = new OrderDetailsView
			{
				DataContext = vm,
				Owner = Application.Current.MainWindow
			};

			view.ShowDialog();
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
		private static decimal RoundRupee(decimal value)
		{
			return Math.Round(value, MidpointRounding.AwayFromZero);
		}
	}
}


