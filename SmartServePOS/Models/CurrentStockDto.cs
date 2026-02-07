namespace SmartServePOS.Models
{
	public class CurrentStockDto
	{
		public int Id { get; set; }
		public string ItemType { get; set; }
		public int ReferenceId { get; set; }

		public string ItemName { get; set; }   // Filled via JOIN later
		public string Unit { get; set; }
		public string Category { get; set; }
		public decimal CurrentQuantity { get; set; }
		public decimal MinStockLevel { get; set; }
		
		public bool IsLowStock => CurrentQuantity <= MinStockLevel;

		// Display-friendly
		public string DisplayStock => $"{CurrentQuantity:0.##} {Unit}";

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
