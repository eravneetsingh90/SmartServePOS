using SmartServe.Domain.Models;
using SmartServe.Domain.Stores;
using SmartServePOS.Command;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class CurrentStockViewModel : BaseViewModel
	{
		private readonly IStockStore _stockStore;
		public IEnumerable<CurrentStockDto> LowStockItems => _allStocks.Where(x => x.IsLowStock);

		public ObservableCollection<CurrentStockDto> Stocks { get; private set; }

		public ICommand RefreshCommand { get; }

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		private string _searchText;
		public string SearchText
		{
			get => _searchText;
			set
			{
				SetProperty(ref _searchText, value);
				ApplyFilter();
			}
		}

		private List<CurrentStockDto> _allStocks;

		public CurrentStockViewModel(IStockStore stockStore)
		{
			_stockStore = stockStore;

			Stocks = new ObservableCollection<CurrentStockDto>();
			_allStocks = new List<CurrentStockDto>();

			RefreshCommand = new RelayCommand(async _ => await LoadAsync());

			_ = LoadAsync(); // auto-load on page open
		}

		private async Task LoadAsync()
		{
			try
			{
				IsLoading = true;

				var data = await _stockStore.GetCurrentStockAsync();

				_allStocks = data
					.OrderBy(x => x.ItemType)
					.ThenBy(x => x.ItemName)
					.ToList();

				ApplyFilter();
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ApplyFilter()
		{
			Stocks.Clear();

			var filtered = string.IsNullOrWhiteSpace(SearchText)
				? _allStocks
				: _allStocks.Where(x =>
					x.ItemName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

			foreach (var item in filtered)
				Stocks.Add(item);
		}
	}
}
