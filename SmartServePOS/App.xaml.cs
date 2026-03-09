using HandyControl.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartServe.Domain.Dependencies;
using SmartServe.Domain.Services;
using SmartServe.EFCore.Dependencies;
using SmartServePOS.Data;
using SmartServePOS.Dependencies;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using SmartServePOS.Services;
using SmartServePOS.Views;
using System.IO;
using System.Net.NetworkInformation;
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

			// -------------------------------
			// 1️⃣ Ensure DB exists
			// -------------------------------
			var dbPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"SmartServePOS",
				"Data",
				"SmartServePOS.db");

			new DatabaseInitializer(dbPath).Initialize();

			// -------------------------------
			// 2️⃣ Load configuration
			// -------------------------------
			Configuration = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			// -------------------------------
			// 3️⃣ Setup DI (ONLY ONCE)
			// -------------------------------
			var services = new ServiceCollection();
            services.Configure<ApiSettings>(Configuration.GetSection("ApiSettings"));
            services.Configure<POSSettings>(Configuration.GetSection("POS"));
			services.Configure<ThemeSettings>(Configuration.GetSection("Theme"));
			services.AddSingleton(Configuration);
			services.AddSingleton<ISqliteConnectionFactory>(new SqliteConnectionFactory(dbPath));
			services.UseEFCore(Configuration);
			services.UseDomain();
			services.UseApp();
			Services = services.BuildServiceProvider();

			//data sync scheduler
			var scheduler = Services.GetRequiredService<SyncScheduler>();
			_ = scheduler.StartAsync(); // fire & forget

			// -------------------------------
			// 4️⃣ Show UI immediately
			// -------------------------------
			var mainWindow = Services.GetRequiredService<MainWindow>();
			mainWindow.Show();

			var loginView = Services.GetRequiredService<LoginView>();
			mainWindow.Navigate(loginView);

			// -------------------------------
			// 5️⃣ Load local catalog (offline-safe)
			// -------------------------------
			var catalog = Services.GetRequiredService<IPOSCatalogService>();
			await catalog.LoadAsync();

			_ = Task.Run(async () =>
			{
				try
				{
					// small delay helps network stabilize
					await Task.Delay(TimeSpan.FromSeconds(5));

					if (!NetworkInterface.GetIsNetworkAvailable())
						return;

					using var scope = Services.CreateScope();
					var syncService = scope.ServiceProvider
						.GetRequiredService<IMasterDataService>();

					await syncService.SyncAsync();

					// Optional: reload catalog after sync
					await catalog.Refresh();
				}
				catch (Exception ex)
				{
					// TODO: log ex (file / telemetry)
				}
			});
		}
	}
}
