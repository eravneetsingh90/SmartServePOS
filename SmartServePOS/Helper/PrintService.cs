using SmartServePOS.Models;
using SmartServePOS.Views;
using System.Linq;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SmartServePOS.Helper
{
	public class PrintService : IPrintService
	{
		public void PrintBill(BillPrintModel bill, bool showPreview)
		{
			var doc = ReceiptBuilder.Build(bill);

			if (showPreview)
			{
				var preview = new PrintPreviewWindow(doc);
				preview.ShowDialog();
				return;
			}

			var printQueue = FindThermalPrinter();

			if (printQueue == null)
			{
				// fallback (optional)
				var dialog = new PrintDialog();
				if (dialog.ShowDialog() == true)
				{
					dialog.PrintDocument(
						((IDocumentPaginatorSource)doc).DocumentPaginator,
						"SmartServe Bill"
					);
				}
				return;
			}

			var writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
			writer.Write(
				((IDocumentPaginatorSource)doc).DocumentPaginator
			);
		}

		private PrintQueue? FindThermalPrinter()
		{
			var server = new LocalPrintServer();

			// 1️⃣ Try default printer first
			var defaultQueue = server.DefaultPrintQueue;
			if (IsThermal(defaultQueue))
				return defaultQueue;

			// 2️⃣ Otherwise find by name
			return server.GetPrintQueues()
				.FirstOrDefault(IsThermal);
		}

		private bool IsThermal(PrintQueue queue)
		{
			if (queue == null) return false;

			var name = queue.Name.ToUpperInvariant();

			return name.Contains("THERMAL")
				|| name.Contains("TVS")
				|| name.Contains("EPSON")
				|| name.Contains("XPRINTER")
				|| name.Contains("RP3200");
		}
	}
}
