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

		private void OpenIngredientStock_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new IngredientStockView());
		}

		private void OpenStockTransactions_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new StockTransactionsView());
		}
	}
}
