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

		public async void NavigateToBilling(int orderId)
		{
			//var billingView = new BillingView();
			//var billingViewModel = ActivatorUtilities.CreateInstance<BillingViewModel>(
			//	_serviceProvider,
			//	orderId
			//);
			//billingView.DataContext = billingViewModel;

			//if (Application.Current.MainWindow is MainWindow mw)
			//{
			//	mw.MainFrame.Navigate(billingView);
			//}
			//await billingViewModel.LoadOrderAsync();

			
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
			await billingViewModel.LoadOrderAsync(orderId);
		}
	}

}
