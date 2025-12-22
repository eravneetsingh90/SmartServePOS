using SmartServePOS.Models;

namespace SmartServePOS.Helper
{
	public interface IPrintService
	{
		void PrintBill(BillPrintModel bill, bool showPreview);
	}

}
