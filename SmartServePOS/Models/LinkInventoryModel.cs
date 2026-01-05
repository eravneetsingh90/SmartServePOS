using System.Windows.Media;

namespace SmartServePOS.Models
{
	public class LinkInventoryModel
	{
		public int VariantId { get; set; }

		public string VariantName { get; set; }
		public string ProductName { get; set; }
		public string CategoryName { get; set; }

		public decimal Price { get; set; }

		// Stock state
		public bool IsStockTracked { get; set; }

		// UI helpers
		public string StockStatus => IsStockTracked ? "Tracked" : "Not Tracked";

		public Brush StockStatusColor =>
			IsStockTracked ? Brushes.Green : Brushes.Gray;

		public string ActionText =>
			IsStockTracked ? "Remove from Stock" : "Add to Stock";
	}

}
