using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class AddStockViewModel : BaseViewModel
	{
		private readonly IStockStore _stockStore;
		private readonly IStockService _stockService;

		public ObservableCollection<StockItemLookupModel> StockItems { get; }

		private StockItemLookupModel _selectedStockItem;
		public StockItemLookupModel SelectedStockItem
		{
			get => _selectedStockItem;
			set => SetProperty(ref _selectedStockItem, value);
		}

		private decimal _quantity;
		public decimal Quantity
		{
			get => _quantity;
			set => SetProperty(ref _quantity, value);
		}

		private string _selectedReason;
		public string SelectedReason
		{
			get => _selectedReason;
			set => SetProperty(ref _selectedReason, value);
		}

		public List<string> Reasons { get; }

		private bool _isSaving;
		public bool IsSaving
		{
			get => _isSaving;
			set => SetProperty(ref _isSaving, value);
		}

		public ICommand SaveCommand { get; }
		public ICommand ResetCommand { get; }

		public AddStockViewModel(
			IStockStore stockStore,
			IStockService stockService)
		{
			_stockStore = stockStore;
			_stockService = stockService;

			StockItems = new ObservableCollection<StockItemLookupModel>();

			Reasons = new List<string>
		{
			"PURCHASE",
			"OPENING",
			"MANUAL"
		};

			SaveCommand = new RelayCommand(async _ => await SaveAsync(), CanSave);
			ResetCommand = new RelayCommand(Reset);

			_ = LoadStockItemsAsync();
		}
		private async Task LoadStockItemsAsync()
		{
			var items = await _stockStore.GetStockAsync();

			StockItems.Clear();

			foreach (var item in items)
			{
				StockItems.Add(new StockItemLookupModel
				{
					StockItemId = item.Id,
					ItemType = item.ItemType,
					ReferenceId = item.ReferenceId,
					Unit = item.Unit,
					DisplayName = $"{item.ItemType} - {item.ReferenceId}"
				});
			}
		}
		private async Task SaveAsync()
		{
			if (SelectedStockItem == null)
				return;

			try
			{
				IsSaving = true;

				//await _stockService.AddStockAsync(
				//	SelectedStockItem.ItemType,
				//	SelectedStockItem.ReferenceId,
				//	Quantity,
				//	SelectedReason);

				Reset(null);
			}
			finally
			{
				IsSaving = false;
			}
		}
		private bool CanSave(object? obj)
		{
			return SelectedStockItem != null
				&& Quantity > 0
				&& !string.IsNullOrWhiteSpace(SelectedReason)
				&& !IsSaving;
		}
		private void Reset(object? obj)
		{
			SelectedStockItem = null;
			Quantity = 0;
			SelectedReason = null;
		}

	}
}
