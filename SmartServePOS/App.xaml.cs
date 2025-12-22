using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Dependencies;
using SmartServe.Domain.Services;
using SmartServe.EFCore.Dependencies;
using SmartServePOS.Helper;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;
using System.Windows;

namespace SmartServePOS
{
	public partial class App : Application
	{
		public static IServiceProvider Services { get; private set; } = null!;
		public static IConfiguration Configuration { get; private set; } = null!;

		protected override async void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.Build();

			// Setup DI
			var services = new ServiceCollection();

			// EF Core DbContext (reused extension from EFCore project)
			services.UseEFCore(Configuration);

			// Stores (EFCore)
			services.UseDomain();

			// WPF Views & ViewModels
			services.AddSingleton<LoginView>();
			services.AddSingleton<LoginViewModel>();
			services.AddTransient<TableView>();
			services.AddSingleton<TableViewModel>();
			services.AddSingleton<MainWindow>();
			services.AddSingleton<MainViewModel>();
			services.AddSingleton<BillingView>();
			services.AddSingleton<BillingViewModel>();

			services.AddSingleton<MenuManagementView>();
			//
			services.AddScoped<IPrintService, PrintService>();

			Services = services.BuildServiceProvider();

			// Resolve main window (do not show yet) and register it as the application's main window.
			var mainWindow = Services.GetRequiredService<MainWindow>();
			this.MainWindow = mainWindow;

			// Resolve login view and show it modally. LoginView will close itself on success and show the MainWindow.
			var loginView = Services.GetRequiredService<LoginView>();
			loginView.ShowDialog();

			Services = services.BuildServiceProvider();

			//Load catalog ONCE
			var catalog = Services.GetRequiredService<ICatalogService>();
			await catalog.LoadAsync();
		}
	}
}
