using SmartServePOS.ViewModels;

namespace SmartServePOS.Models
{
	public class IngredientStockSetupModel : BaseViewModel
	{
		private bool _isStockTracked;

		public int IngredientId { get; set; }
		public string IngredientName { get; set; }
		public string Unit { get; set; }

		public bool IsStockTracked
		{
			get => _isStockTracked;
			set
			{
				SetProperty(ref _isStockTracked, value);
				OnPropertyChanged(nameof(ActionText));
				OnPropertyChanged(nameof(StockStatus));
			}
		}

		public string ActionText =>
			IsStockTracked ? "Remove from Stock" : "Add to Stock";

		public string StockStatus =>
			IsStockTracked ? "Tracked" : "Not Tracked";
	}

}
