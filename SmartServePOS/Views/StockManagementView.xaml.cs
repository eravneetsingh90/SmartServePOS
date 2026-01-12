using SmartServePOS.Helper;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	/// <summary>
	/// Interaction logic for StockManagementView.xaml
	/// </summary>
	public partial class StockManagementView : Page
	{
		private readonly INavigationService _navigationService;
		public StockManagementView(INavigationService navigationService)
		{
			InitializeComponent();
			_navigationService = navigationService;
		}
		private void OpenIceCreamStock_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new CurrentStockView());
		}

		private void OpenAddStock_Click(object sender, MouseButtonEventArgs e)
		{
			_navigationService.NavigateToAddStockView();
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
