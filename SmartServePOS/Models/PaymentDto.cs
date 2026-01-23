namespace SmartServePOS.Models
{
	public class PaymentDto
	{
		public int Id { get; set; }

		public int OrderId { get; set; }

		public string Mode { get; set; }
		// CASH | UPI | CARD

		public decimal Amount { get; set; }
		public decimal PartPaymentCash { get; set; }

		public string Status { get; set; }
		// PAID | FAILED

		public DateTime CreatedAt { get; set; }

		// 🔄 Sync
		public bool IsSynced { get; set; }
	}
}
