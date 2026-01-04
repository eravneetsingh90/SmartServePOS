using SmartServe.Domain.Stores;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class StockViewModel : BaseViewModel
	{
		private readonly IStockStore _stockStore;

		public StockViewModel(IStockStore stockStore)
		{
			_stockStore = stockStore;

			Items = new ObservableCollection<StockModel>();

			//AddStockCommand = new RelayCommand(AddStock);
			//AdjustStockCommand = new RelayCommand(
			//	AdjustStock,
			//	() => SelectedItem != null
			//);

			LoadStockAsync();
		}

		// ================= PROPERTIES =================

		public ObservableCollection<StockModel> Items { get; }

		private StockModel? _selectedItem;
		public StockModel? SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}

		private string _searchText = string.Empty;
		public string SearchText
		{
			get => _searchText;
			set
			{
				_searchText = value;
				OnPropertyChanged();
				ApplySearch();
			}
		}

		// ================= COMMANDS =================

		public ICommand AddStockCommand { get; }
		public ICommand AdjustStockCommand { get; }

		// ================= LOAD =================

		private async void LoadStockAsync()
		{
			Items.Clear();

			//var stock = await _stockStore.GetAllAsync();

			//foreach (var item in stock)
			//{
			//	Items.Add(new StockModel
			//	{
			//		BrandName = item?.Variant?.Brand?.Name ?? string.Empty,
			//		VariantName = item?.Variant.Name ?? string.Empty,
			//		Quantity = item?.Quantity ?? 0,
			//		Status = item?.Quantity > 0 ? "In Stock" : "Out of Stock"
			//	});
			//}
		}

		// ================= SEARCH =================

		private void ApplySearch()
		{
			// Simple approach for now:
			// Reload filtered data from store (safe & predictable)

			LoadStockAsync();

			if (string.IsNullOrWhiteSpace(SearchText))
				return;

			var filtered = Items
				.Where(x =>
					x.BrandName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
					x.VariantName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
				.ToList();

			Items.Clear();
			foreach (var item in filtered)
				Items.Add(item);
		}

		// ================= ACTIONS =================

		private void AddStock()
		{
			// This will open AddStock dialog later
			// Dialog returns → refresh stock
			LoadStockAsync();
		}

		private void AdjustStock()
		{
			if (SelectedItem == null)
				return;

			// This will open AdjustStock dialog later
			// After adjustment → refresh stock
			LoadStockAsync();
		}
	}
}
