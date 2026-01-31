using SmartServe.Domain.Models;
using SmartServe.Domain.Stores;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{

	public class CurrentStockViewModel : BaseViewModel
	{
		private int _totalItems;
		public int TotalItems
		{
			get => _totalItems;
			set { _totalItems = value; OnPropertyChanged(); }
		}

		private int _inStockCount;
		public int InStockCount
		{
			get => _inStockCount;
			set { _inStockCount = value; OnPropertyChanged(); }
		}

		private int _lowStockCount;
		public int LowStockCount
		{
			get => _lowStockCount;
			set { _lowStockCount = value; OnPropertyChanged(); }
		}

		private int _outOfStockCount;
		public int OutOfStockCount
		{
			get => _outOfStockCount;
			set { _outOfStockCount = value; OnPropertyChanged(); }
		}

		// ================= FILTERS =================

		private string _searchText;
		public string SearchText
		{
			get => _searchText;
			set { _searchText = value; OnPropertyChanged(); }
		}

		private string _selectedCategory;
		public string SelectedCategory
		{
			get => _selectedCategory;
			set { _selectedCategory = value; OnPropertyChanged(); }
		}

		private StockStatus? _selectedStatus;
		public StockStatus? SelectedStatus
		{
			get => _selectedStatus;
			set { _selectedStatus = value; OnPropertyChanged(); }
		}

		// ================= STOCK LIST =================

		public ObservableCollection<CurrentStockDto> StockItems { get; set; }

		// ================= CONSTRUCTOR =================

		public CurrentStockViewModel()
		{
			StockItems = new ObservableCollection<CurrentStockDto>();
		}
		public async Task Load()
		{
			StockItems.Clear();
			LoadMockData();
			CalculateSummary();
		}
		// ================= DATA LOADING =================

		private void LoadMockData()
		{
			StockItems.Add(new CurrentStockDto
			{
				ItemName = "Vanilla Ice Cream",
				Category = "Ice Cream",
				CurrentStock = 2.4m,
				Unit = "kg",
				Status = StockStatus.LowStock
			});

			StockItems.Add(new CurrentStockDto
			{
				ItemName = "Chocolate Cone",
				Category = "Cone",
				CurrentStock = 0,
				Unit = "pcs",
				Status = StockStatus.OutOfStock
			});

			StockItems.Add(new CurrentStockDto
			{
				ItemName = "Strawberry Scoop",
				Category = "Ice Cream",
				CurrentStock = 5.6m,
				Unit = "kg",
				Status = StockStatus.InStock
			});
		}

		// ================= SUMMARY CALC =================

		private void CalculateSummary()
		{
			TotalItems = StockItems.Count;
			InStockCount = StockItems.Count(x => x.Status == StockStatus.InStock);
			LowStockCount = StockItems.Count(x => x.Status == StockStatus.LowStock);
			OutOfStockCount = StockItems.Count(x => x.Status == StockStatus.OutOfStock);
		}
	}

}
