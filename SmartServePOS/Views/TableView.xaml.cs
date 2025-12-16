using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.Models;
using SmartServePOS.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	public partial class TableView : Page
	{
		public TableView()
		{
			InitializeComponent();

			// Don't overwrite an externally-provided DataContext (DI) - only set a default for runtime.
			if (DataContext == null)
			{
				// Prefer constructor injection; fall back to resolving from the application's service provider.
				if (App.Services is not null)
				{
					DataContext = App.Services.GetService<TableViewModel>();
				}
				// If still null, leave DataContext alone (designer may provide a d:DataContext).
			}
		}

		private void TableButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.CommandParameter is GetTableViewDto table)
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
