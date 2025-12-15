using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Dependencies;
using SmartServe.EFCore.Dependencies;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;
using System.Windows;

namespace SmartServePOS
{
	public partial class App : Application
	{
		public static IServiceProvider Services { get; private set; } = null!;
		public static IConfiguration Configuration { get; private set; } = null!;

		protected override void OnStartup(StartupEventArgs e)
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
			services.AddSingleton<MainWindow>();
			services.AddSingleton<MainViewModel>();
			Services = services.BuildServiceProvider();

			// Start Window
			var loginView = Services.GetRequiredService<LoginView>();
			loginView.Show();
		}
	}
}
