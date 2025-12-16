using System.Windows;
using System.Windows.Controls;
using SmartServePOS.ViewModels;

namespace SmartServePOS.Views
{
	public partial class TableView : Page
	{
		public TableView()
		{
			InitializeComponent();

			// Don't overwrite an externally-provided DataContext (DI) - only set a default for design/runtime.
			if (DataContext == null)
			{
				DataContext = new TableViewModel();
			}
		}

		private void TableButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.CommandParameter is TableItem table)
			{
				//Create billing view and pass the selected table via its DataContext or constructor.
				var billingView = new BillingView();
				billingView.DataContext = new BillingViewModel();
				//If BillingViewModel had a SelectedTable property, you could set it here.
				// Navigate using the MainWindow's frame so it replaces the current page.
				if (Application.Current.MainWindow is MainWindow mw)
				{
					mw.MainFrame.Navigate(billingView);
				}
			}
		}
	}
}
