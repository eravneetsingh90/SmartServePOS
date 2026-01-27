namespace SmartServePOS.Helper
{
	public interface INavigationService
	{
		void NavigateTo<T>();
		void NavigateToAddStockView();
		//void NavigateToBillingView(int orderId,int tableId);
		Task NavigateToBillingView(int orderId, int tableId);
		void NavigateToTableView();
		void NavigateToCategoryView();
		void NavigateToProductView();
		void NavigateToProductVariantView();
		void NavigateToMenuManagementView();
		void NavigateToStockManagementView();
		void NavigateToDashboardView();
		void OpenOrderDetailsDialog(int id);
		
	}
}
