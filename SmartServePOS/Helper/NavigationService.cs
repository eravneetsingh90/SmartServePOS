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
		public async Task NavigateToBillingView(int orderId, int tableId)
		{
			BillingViewModel billingViewModel;

			if (App.Services is not null)
			{
				billingViewModel = App.Services.GetService<BillingViewModel>()
					?? throw new InvalidOperationException("BillingViewModel not registered.");
			}
			else
			{
				throw new InvalidOperationException("DI container not available.");
			}

			var billingView = new BillingView
			{
				DataContext = billingViewModel
			};

			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(billingView);
			}

			// Load AFTER navigation, still safe
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
		public async void NavigateToDashboardView()
		{
			DashboardViewModel viewModel = null;
			if (App.Services is not null)
			{
				viewModel = App.Services.GetService<DashboardViewModel>();
			}
			else
			{
				throw new InvalidOperationException("DashboardViewModel dependencies must be provided via DI.");
			}

			var view = new DashboardView();
			view.DataContext = viewModel;
			if (Application.Current.MainWindow is MainWindow mw)
			{
				mw.MainFrame.Navigate(view);
			}
			await viewModel.InitializeAsync();
		}

		public void OpenOrderDetailsDialog(int id)
		{
			//throw new NotImplementedException();
		}
	}

}
