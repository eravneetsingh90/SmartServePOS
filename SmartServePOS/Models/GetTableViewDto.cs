namespace SmartServePOS.Models
{
	public class GetTableViewDto
	{
		public int TableId { get; set; }
		public string DisplayName { get; set; }

		public int? OrderId { get; set; }

		public decimal Amount { get; set; }

		public string StatusCode { get; set; }
		public string StatusName { get; set; }
		public string ColorHex { get; set; }
	}

}
