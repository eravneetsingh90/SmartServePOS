namespace SmartServePOS.Models
{
	public class ProductModel
	{
		public int ProductId { get; set; }

		public string Name { get; set; } = null!;

		public int? CategoryId { get; set; }

		public bool? IsActive { get; set; }

		public int DisplayOrder { get; set; }

		public DateTime? CreatedAt { get; set; }
	}
}
