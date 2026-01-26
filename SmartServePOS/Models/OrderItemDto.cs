using SmartServe.Domain.Models;
using System;

namespace SmartServePOS.Models
{
	public class OrderItemDto
	{
		public int Id { get; set; }

		public int OrderId { get; set; }
		public int VariantId { get; set; }

		public int Quantity { get; set; }

		public decimal PriceSnapshot { get; set; }
		public decimal DiscountAmount { get; set; }

		// 🔹 Derived (not stored, but useful for UI)
		public decimal LineTotal =>
			(PriceSnapshot * Quantity) - DiscountAmount;
		public string ProductName { get; set; }
		public string VariantName { get; set; }
		public string DisplayName =>
		string.IsNullOrWhiteSpace(VariantName)
			? ProductName
			: $"{ProductName} ({VariantName})";
		public virtual ProductVariantDto Variant { get; set; }
	}
}
