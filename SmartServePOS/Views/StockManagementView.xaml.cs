using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	/// <summary>
	/// Interaction logic for StockManagementView.xaml
	/// </summary>
	public partial class StockManagementView : Page
	{
		public StockManagementView()
		{
			InitializeComponent();
		}
		private void OpenIceCreamStock_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new CurrentStockView());
		}

		private void OpenAddStock_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new AddStockView());
		}

		private void OpenStockTransactions_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new StockTransactionsView());
		}

		private void OpenLinkInventory_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new LinkInventoryView());
		}
		
	}
}
