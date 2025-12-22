namespace SmartServePOS.Models
{
	public class BillPrintItem
	{
		public string Name { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal Total => Quantity * UnitPrice;
	}

}
