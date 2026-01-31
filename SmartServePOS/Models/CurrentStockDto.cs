namespace SmartServePOS.Models
{
	public class CurrentStockDto
	{
		public int Id { get; set; }
		public string ItemName { get; set; }
		public string Category { get; set; }

		// Raw stock
		public decimal CurrentStock { get; set; }
		public string Unit { get; set; }

		// Display-friendly
		public string DisplayStock => $"{CurrentStock:0.##} {Unit}";

		// Status
		public StockStatus Status { get; set; }

		public string StockStatusText =>
			Status switch
			{
				StockStatus.InStock => "In Stock",
				StockStatus.LowStock => "Low Stock",
				StockStatus.OutOfStock => "Out of Stock",
				_ => "Unknown"
			};

		public string StatusHint =>
			Status switch
			{
				StockStatus.LowStock => "Below minimum",
				StockStatus.OutOfStock => "Stop selling",
				_ => string.Empty
			};
	}
	public enum StockStatus
	{
		InStock,
		LowStock,
		OutOfStock
	}

}
