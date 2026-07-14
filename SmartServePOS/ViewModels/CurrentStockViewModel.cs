using AutoMapper;
using SmartServe.Domain.Interfaces;
using SmartServe.Domain.Models;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class CurrentStockViewModel : BaseViewModel
	{
		public ICommand SyncUntrackedOrdersCommand { get; }
		private readonly IStockService _stockService;
		private readonly IMapper _mapper;

		// ================= SUMMARY =================

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

		// ================= DATA =================

		public ObservableCollection<CurrentStockDto> StockItems { get; }

		// ================= CONSTRUCTOR =================

		public CurrentStockViewModel(IStockService stockService, IMapper mapper)
		{
			_stockService = stockService;
			_mapper = mapper;
			StockItems = new ObservableCollection<CurrentStockDto>();
			SyncUntrackedOrdersCommand = new AsyncRelayCommand(SyncUntrackedOrdersAsync);
		}

		// ================= LOAD =================

		public async Task LoadAsync()
		{
			StockItems.Clear();

			var items = await _stockService.GetCurrentStockAsync();

			foreach (var item in items)
			{
				StockItems.Add(_mapper.Map<CurrentStockDto>(item));
			}

			CalculateSummary();
		}

		private async Task SyncUntrackedOrdersAsync()
		{
			await _stockService.ProcessUntrackedOrdersAsync();
			await LoadAsync();
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
