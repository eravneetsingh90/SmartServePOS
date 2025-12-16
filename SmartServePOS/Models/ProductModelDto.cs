namespace SmartServePOS.Models
{
	public class ProductModelDto
	{
		public int ProductId { get; set; }     // product_id
		public string Name { get; set; }        // name

		public int CategoryId { get; set; }     // category_id
		public int ServingTypeId { get; set; } // serving_type_id
		public int? BrandId { get; set; }       // brand_id (nullable)
		public int? FlavorId { get; set; }      // flavor_id (nullable)

		public decimal Price { get; set; }     // price
	}

}
