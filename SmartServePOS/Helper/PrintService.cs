using SmartServePOS.Models;
using SmartServePOS.Views;
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

			var dialog = new PrintDialog();
			if (dialog.ShowDialog() == true)
			{
				dialog.PrintDocument(
					((IDocumentPaginatorSource)doc).DocumentPaginator,
					"SmartServe Bill"
				);
			}
		}
	}

}
