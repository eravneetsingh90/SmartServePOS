namespace SmartServePOS.Helper
{
	public interface INavigationService
	{
		void NavigateToBilling(int orderId,int tableId);
		void NavigateToTable();
	}
}
