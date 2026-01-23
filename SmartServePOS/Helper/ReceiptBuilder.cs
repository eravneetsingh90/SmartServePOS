using SmartServePOS.Models;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SmartServePOS.Helper
{
	public static class ReceiptBuilder
	{
		private const int LineWidth = 42; // Perfect for 80mm

		public static FlowDocument Build(BillPrintModel bill)
		{
			var doc = new FlowDocument
			{
				FontFamily = new FontFamily("Consolas"), // IMPORTANT
				FontSize = 11,
				PageWidth = 300, // MUST match LineWidth logic
				PagePadding = new Thickness(10, 5, 10, 5)
			};

			// ===== HEADER =====
			doc.Blocks.Add(Center("RETAIL INVOICE", true));
			doc.Blocks.Add(Center(bill.ShopName, true,14));
			doc.Blocks.Add(Center(bill.Address));
			doc.Blocks.Add(Line());

			// ===== META =====
			doc.Blocks.Add(Text(
				Pad($"Bill No.: {bill.BillNo}", 20) +
				$"   Date: {bill.PrintedAt:dd/MM/yy}"
			));

			//doc.Blocks.Add(Text(
			//	Pad(bill.PrintedAt.ToString("HH:mm"), 20) +
			//	$"Bill No.: {bill.BillNo}"
			//));

			//doc.Blocks.Add(Text(
			//	Pad($"Cashier: {bill.Cashier}", 20)
			//));

			doc.Blocks.Add(Line());

			// ===== TABLE HEADER =====
			doc.Blocks.Add(Text(
				Pad("Item", 22) +
				Pad("Qty", 4) +
				Pad("Rate", 7) +
				"Amt"
			));

			doc.Blocks.Add(Line());

			// ===== ITEMS =====
			foreach (var item in bill.Items)
			{
				doc.Blocks.Add(Text(
					Pad(Trim(item.Name, 22), 22) +
					Pad(item.Quantity.ToString(), 4) +
					Pad(item.UnitPrice.ToString("0.00"), 7) +
					item.Total.ToString("0.00")
				));

				if (item.Name.Length > 22)
				{
					doc.Blocks.Add(Text(
						"  " + item.Name[22..]
					));
				}
				// --- Variant line (separate, indented, full width)
				if (!string.IsNullOrWhiteSpace(item.VariantName))
				{
					doc.Blocks.Add(Text(
						"  - " + Trim(item.VariantName, LineWidth - 4)
					));
				}

			}

			doc.Blocks.Add(Line());

			// ===== TOTALS =====
			doc.Blocks.Add(Text(
				Pad($"Total Qty: {bill.Items.Sum(i => i.Quantity)}", 22) +
				$"Sub Total {bill.SubTotal:0.00}"
			));

			if (bill.Discount > 0)
			{
				doc.Blocks.Add(Text(
					$"Discount {bill.DiscountLabel} ({bill.Discount:0.00})"
				));
			}

			doc.Blocks.Add(Line());

			// ===== GRAND TOTAL =====
			doc.Blocks.Add(
				Center($"GRAND TOTAL ₹ {bill.GrandTotal:0.00}", true, 15)
			);

			doc.Blocks.Add(Line());

			doc.Blocks.Add(Center("Thanks & Visit Again"));

			return doc;
		}

		// ===== HELPERS =====

		private static Paragraph Text(string text)
			=> new Paragraph(new Run(text))
			{
				Margin = new Thickness(0),
				LineHeight = 14
			};

		private static Paragraph Center(string text, bool bold = false, double size = 11)
			=> new Paragraph(new Run(text))
			{
				FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
				FontSize = size,
				TextAlignment = TextAlignment.Center,
				Margin = new Thickness(0),
				LineHeight = size + 2
			};


		private static Paragraph Line()
			=> new Paragraph(new Run(new string('-', LineWidth)))
			{
				Margin = new Thickness(0)
			};

		private static string Pad(string text, int width)
			=> text.Length >= width ? text[..width] : text.PadRight(width);

		private static string Trim(string text, int width)
			=> text.Length > width ? text[..width] : text;
	}

}
