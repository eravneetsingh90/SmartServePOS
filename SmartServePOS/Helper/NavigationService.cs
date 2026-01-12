using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Services;
using SmartServe.EFCore.Models;
using SmartServePOS.Models;
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
		public async void NavigateToAddStockView()
		{
			AddStockViewModel viewModel = null;
			if (App.Services is not null)
			{
				viewModel = App.Services.GetService<AddStockViewModel>();
			}
			else
			{
				throw new InvalidOperationException("AddStockViewModel dependencies must be provided via DI.");
			}

			var view = new AddStockView();
			view.DataContext = viewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
			await viewModel.InitializeAsync();
		}
		public async void NavigateToBillingView(int orderId,int tableId)
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

		public async void NavigateToTableView()
		{
			TableViewModel tableViewModel = null;
			if (App.Services is not null)
			{
				tableViewModel = App.Services.GetService<TableViewModel>();
			}
			else
			{
				throw new InvalidOperationException("TableViewModel dependencies must be provided via DI.");
			}

			var tableView = new TableView();
			tableView.DataContext = tableViewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(tableView);
			}

		}
		public async void NavigateToCategoryView()
		{
			CategoryViewModel viewModel = null;
			if (App.Services is not null)
			{
				viewModel = App.Services.GetService<CategoryViewModel>();
			}
			else
			{
				throw new InvalidOperationException("CategoryViewModel dependencies must be provided via DI.");
			}

			var view = new CategoryView();
			view.DataContext = viewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
			await viewModel.Initialize();
		}
		public async void NavigateToProductView()
		{
			ProductViewModel viewModel = null;
			if (App.Services is not null)
			{
				viewModel = App.Services.GetService<ProductViewModel>();
			}
			else
			{
				throw new InvalidOperationException("ProductViewModel dependencies must be provided via DI.");
			}

			var view = new ProductView();
			view.DataContext = viewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
			await viewModel.Initialize();
		}
		public async void NavigateToProductVariantView()
		{
			ProductVariantViewModel viewModel = null;
			if (App.Services is not null)
			{
				viewModel = App.Services.GetService<ProductVariantViewModel>();
			}
			else
			{
				throw new InvalidOperationException("ProductVariantViewModel dependencies must be provided via DI.");
			}

			var view = new ProductVariantView();
			view.DataContext = viewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
			await viewModel.Initialize();
		}

		public void NavigateToMenuManagementView()
		{
			var view = _serviceProvider.GetService<MenuManagementView>();
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
		}
		public void NavigateToStockManagementView()
		{
			var view = _serviceProvider.GetService<StockManagementView>();
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
		}
	}

}
