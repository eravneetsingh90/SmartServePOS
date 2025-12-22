using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.Helper;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;

namespace SmartServePOS.Dependencies
{
	public static class DependencyExtensions
	{
		public static IServiceCollection UseApp(
			this IServiceCollection services)
		{
			//Views and ViewModels
			services.AddSingleton<LoginView>();
			services.AddSingleton<LoginViewModel>();
			services.AddTransient<TableView>();
			services.AddSingleton<TableViewModel>();
			services.AddSingleton<MainWindow>();
			services.AddSingleton<MainViewModel>();
			services.AddSingleton<BillingView>();
			services.AddSingleton<BillingViewModel>();
			services.AddSingleton<MenuManagementView>();

			services.AddSingleton<CategoryView>();
			services.AddSingleton<CategoryViewModel>();
			services.AddSingleton<ProductView>();
			services.AddSingleton<ProductViewModel>();

			services.AddScoped<IPrintService, PrintService>();

			return services;
		}
	}
}
