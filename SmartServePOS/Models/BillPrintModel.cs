namespace SmartServePOS.Models
{
	public class BillPrintModel
	{
		public string ShopName { get; set; }
		public string Address { get; set; }

		public string BillNo { get; set; }
		public string TableName { get; set; }
		public string Cashier { get; set; }

		public DateTime PrintedAt { get; set; }

		public List<BillPrintItem> Items { get; set; } = new();

		public decimal SubTotal { get; set; }
		public decimal Discount { get; set; }
		public string DiscountLabel { get; set; } // e.g. "10%"
		public decimal GrandTotal { get; set; }
	}

}
