namespace SmartServePOS.Models
{
	public class StockModel
	{
		public string BrandName { get; set; } = string.Empty;
		public string VariantName { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public string Status { get; set; } = string.Empty;
	}
}
