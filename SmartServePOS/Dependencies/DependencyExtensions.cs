using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.Helper;
using SmartServePOS.Mapping;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;

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
			services.AddSingleton<CurrentStockView>();
			services.AddSingleton<CurrentStockViewModel>();

			services.AddScoped<IPrintService, PrintService>();
			services.AddScoped<INotificationService, HandyNotificationService>();
			services.AddScoped<IDialogService, HandyDialogService>();
			services.AddScoped<INavigationService, NavigationService>(); 

			return services;
		}
	}
}
