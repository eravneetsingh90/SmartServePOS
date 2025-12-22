using SmartServePOS.Models;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SmartServePOS.Helper
{
	public static class ReceiptBuilder
	{
		private const int LineWidth = 32; // 58–80mm printer safe

		public static FlowDocument Build(BillPrintModel bill)
		{
			var doc = new FlowDocument
			{
				FontFamily = new FontFamily("Consolas"),
				FontSize = 11,
				PageWidth = 300,
				PagePadding = new Thickness(10)
			};

			// ===== HEADER =====
			doc.Blocks.Add(Center("RETAIL INVOICE", true));
			doc.Blocks.Add(Center(bill.ShopName, true));
			doc.Blocks.Add(Center(bill.Address));
			doc.Blocks.Add(Line());

			// ===== META =====
			doc.Blocks.Add(Text(
				Pad($"Date: {bill.PrintedAt:dd/MM/yy}", 20) +
				$"Dine In: {bill.TableName}"
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
				Pad("Item", 16) +
				Pad("Qty", 4) +
				Pad("Price", 6) +
				"Amt"
			));

			doc.Blocks.Add(Line());

			// ===== ITEMS =====
			foreach (var item in bill.Items)
			{
				doc.Blocks.Add(Text(
					Pad(Trim(item.Name, 16), 16) +
					Pad(item.Quantity.ToString(), 4) +
					Pad(item.UnitPrice.ToString("0.00"), 6) +
					item.Total.ToString("0.00")
				));

				if (item.Name.Length > 16)
				{
					doc.Blocks.Add(Text(
						$"({item.Name[16..]})"
					));
				}
			}

			doc.Blocks.Add(Line());

			// ===== TOTALS =====
			doc.Blocks.Add(Text(
				Pad($"Total Qty: {bill.Items.Sum(i => i.Quantity)}", 20) +
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
			doc.Blocks.Add(Center($"Grand Total ₹ {bill.GrandTotal:0.00}", true, 14));
			doc.Blocks.Add(Line());

			doc.Blocks.Add(Center("Thanks & Visit Again"));

			return doc;
		}

		// ===== HELPERS =====

		private static Paragraph Text(string text)
			=> new Paragraph(new Run(text));

		private static Paragraph Center(string text, bool bold = false, double size = 11)
			=> new Paragraph(new Run(text))
			{
				FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
				FontSize = size,
				TextAlignment = TextAlignment.Center
			};

		private static Paragraph Line()
			=> new Paragraph(new Run(new string('-', LineWidth)));

		private static string Pad(string text, int width)
			=> text.Length >= width ? text[..width] : text.PadRight(width);

		private static string Trim(string text, int width)
			=> text.Length > width ? text[..width] : text;
	}

}
