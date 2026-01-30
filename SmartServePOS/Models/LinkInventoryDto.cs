using SmartServePOS.ViewModels;
using System.Windows.Media;

namespace SmartServePOS.Models
{
	public class LinkInventoryDto : BaseViewModel
	{
		public int Id { get; set; }
		public int VariantId { get; set; }
		public string ItemType { get; set; }
		public string Unit { get; set; }
		public decimal MinStockLevel { get; set; } = 0;
		public string VariantName { get; set; }
		public string ProductName { get; set; }
		public string CategoryName { get; set; }

		public decimal Price { get; set; }

		private bool _isStockTracked;
		public bool IsStockTracked
		{
			get => _isStockTracked;
			set
			{
				if (_isStockTracked == value)
					return;

				_isStockTracked = value;

				OnPropertyChanged(nameof(IsStockTracked));
				OnPropertyChanged(nameof(ActionText));
				OnPropertyChanged(nameof(StockStatus));
				OnPropertyChanged(nameof(StockStatusColor));
			}
		}
		// UI helpers
		public string StockStatus => IsStockTracked ? "Tracked" : "Not Tracked";

		public Brush StockStatusColor =>
			IsStockTracked ? Brushes.Green : Brushes.Gray;

		public string ActionText =>
			IsStockTracked ? "Remove from Stock" : "Add to Stock";

		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
	}

}
