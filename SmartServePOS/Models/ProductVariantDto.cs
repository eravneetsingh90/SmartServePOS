using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartServePOS.Models
{
	public class ProductVariantDto
	{
		public int Id { get; set; }              // ServerId
		public int ProductId { get; set; }       // Server ProductId
		public int? BrandId { get; set; }        // Server BrandId (nullable)
		public string VariantName { get; set; }
		public decimal Price { get; set; }
		public bool IsActive { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

}
