namespace SmartServePOS.Models
{
	public class ProductVariantModelDto
	{
		public int VariantId { get; set; }
		public int ProductId { get; set; }
		public string VariantName { get; set; }   // Single Scoop, Double Scoop
		public decimal Price { get; set; }
		public bool TracksStock { get; set; }
	}

}
