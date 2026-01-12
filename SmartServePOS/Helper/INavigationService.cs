namespace SmartServePOS.Helper
{
	public interface INavigationService
	{
		void NavigateToAddStockView();
		void NavigateToBillingView(int orderId,int tableId);
		void NavigateToTableView();
		void NavigateToCategoryView();
		void NavigateToProductView();
		void NavigateToProductVariantView();
		void NavigateToMenuManagementView();
		void NavigateToStockManagementView();
	}
}
