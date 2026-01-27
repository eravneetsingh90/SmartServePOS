using System;
using System.Collections.Generic;

namespace SmartServePOS.Models
{
	public class OrderDto
	{
		// SQLite identity (offline primary key)
		public int Id { get; set; }

		public string OrderNumber { get; set; }

		public string OrderType { get; set; }
		// DINE_IN | DELIVERY | PICKUP

		public int? TableId { get; set; }
		public int StatusId { get; set; }
		public decimal OriginalAmount { get; set; }
		public decimal TotalAmount { get; set; }

		public string DiscountType { get; set; }
		// FLAT | PERCENT

		public decimal DiscountValue { get; set; }
		public string DiscountReason { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? ClosedAt { get; set; }

		// 🔄 Sync fields
		public bool IsSynced { get; set; }
		public DateTime? SyncedOn { get; set; }
		public string SyncError { get; set; }

		// Navigation-style DTOs
		public List<OrderItemDto> OrderItems { get; set; } = new();
		public List<PaymentDto> Payments { get; set; } = new();
	}
}
