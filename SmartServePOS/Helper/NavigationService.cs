using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Services;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;
using System.Windows;

namespace SmartServePOS.Helper
{
	public class NavigationService : INavigationService
	{
		private readonly IServiceProvider _serviceProvider;

		public NavigationService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async void NavigateToBilling(int orderId,int tableId)
		{
			BillingViewModel billingViewModel = null;
			if (App.Services is not null)
			{
				billingViewModel = App.Services.GetService<BillingViewModel>();
			}
			else
			{
				throw new InvalidOperationException("BillingViewModel dependencies must be provided via DI.");
			}

			var billingView = new BillingView();
			billingView.DataContext = billingViewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(billingView);
			}
			await billingViewModel.LoadOrderAsync(orderId, tableId);
		}

		public async void NavigateToTable()
		{
			//TableViewModel tableViewModel = null;
			//if (App.Services is not null)
			//{
			//	tableViewModel = App.Services.GetService<TableViewModel>();
			//}
			//else
			//{
			//	throw new InvalidOperationException("TableViewModel dependencies must be provided via DI.");
			//}

			var tableView = new TableView();
			//tableView.DataContext = tableViewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(tableView);
			}

		}
	}

}
