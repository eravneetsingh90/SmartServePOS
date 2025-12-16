using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartServePOS.Models
{
	public class BillItemModelDto
	{
		public int ProductId { get; set; }     // product_id
		public string ItemName { get; set; }

		public int Quantity { get; set; }
		public decimal PriceSnapshot { get; set; }  // price_snapshot

		public decimal TotalPrice => Quantity * PriceSnapshot;
	}

}
