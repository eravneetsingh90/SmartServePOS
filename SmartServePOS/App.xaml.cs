using HandyControl.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Dependencies;
using SmartServe.Domain.Services;
using SmartServe.EFCore.Dependencies;
using SmartServePOS.Dependencies;
using SmartServePOS.Helper;
using SmartServePOS.Views;
using System.IO;
using System.Windows;

namespace SmartServePOS
{
	public partial class App : Application
	{
		public static IServiceProvider Services { get; private set; } = null!;
		public static IConfiguration Configuration { get; private set; } = null!;

		protected override async void OnStartup(StartupEventArgs e)
		{
			ConfigHelper.Instance.SetLang("en");
			base.OnStartup(e);

			var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"SmartServePOS","Data","SmartServePOS.db");

			new DatabaseInitializer(dbPath).Initialize();

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

			services.UseApp();

			Services = services.BuildServiceProvider();

			var mainWindow = Services.GetRequiredService<MainWindow>();
			mainWindow.Show();

			var loginView = Services.GetRequiredService<LoginView>();
			mainWindow.Navigate(loginView);

			Services = services.BuildServiceProvider();

			//Load catalog ONCE
			var catalog = Services.GetRequiredService<ICatalogService>();
			await catalog.LoadAsync();
		}
	}
}
