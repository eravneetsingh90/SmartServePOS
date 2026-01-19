using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.Data;
using SmartServePOS.Helper;
using SmartServePOS.Mapping;
using SmartServePOS.Services;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;
using System.IO;

namespace SmartServePOS.Dependencies
{
	public static class DependencyExtensions
	{
		public static IServiceCollection UseApp(
			this IServiceCollection services)
		{
			//mapping profiles
			services.AddAutoMapper(typeof(MappingProfile));

			//Views and ViewModels
			services.AddTransient<LoginView>();
			services.AddTransient<LoginViewModel>();
			services.AddTransient<TableView>();
			services.AddTransient<TableViewModel>();
			services.AddSingleton<MainWindow>();
			services.AddSingleton<MainWindowViewModel>();
			services.AddSingleton<BillingView>();
			services.AddTransient<BillingViewModel>();
			services.AddSingleton<MenuManagementView>();
			services.AddSingleton<CategoryView>();
			services.AddSingleton<CategoryViewModel>();
			services.AddSingleton<ProductView>();
			services.AddSingleton<ProductViewModel>();
			services.AddSingleton<ProductVariantView>();
			services.AddSingleton<ProductVariantViewModel>();
			services.AddSingleton<StockManagementView>();
			services.AddSingleton<AddStockView>();
			services.AddSingleton<AddStockViewModel>();
			services.AddSingleton<CurrentStockView>();
			services.AddSingleton<CurrentStockViewModel>();
			services.AddSingleton<LinkInventoryView>();
			services.AddSingleton<LinkInventoryViewModel>();

			services.AddScoped<IPrintService, PrintService>();
			services.AddScoped<INotificationService, HandyNotificationService>();
			services.AddScoped<IDialogService, HandyDialogService>();
			services.AddScoped<INavigationService, NavigationService>();

			services.AddTransient<IMasterDataService, MasterDataService>();
			services.AddTransient<IPOSBillingService, POSBillingService>();
			services.AddSingleton<IPOSCatalogService, POSCatalogService>();
			return services;
		}
	}
}
