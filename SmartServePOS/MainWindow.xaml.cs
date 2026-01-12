using SmartServePOS.Helper;
using SmartServePOS.ViewModels;
using SmartServePOS.Views;
using System.Windows;
using System.Windows.Controls;

namespace SmartServePOS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly INavigationService _navigationService;
		public MainWindow(MainWindowViewModel vm, INavigationService navigationService)
		{
			InitializeComponent();
			DataContext = vm;
			_navigationService = navigationService;
		}

		// Navigate to a Page instance resolved from DI
		public void Navigate(Page page)
		{
			MainFrame.Navigate(page);
		}

		private void NewOrderButton_Click(object sender, RoutedEventArgs e)
		{
			_navigationService.NavigateToTableView();
		}

		private void MenuManagementButton_Click(object sender, RoutedEventArgs e)
		{
			_navigationService.NavigateToMenuManagementView();
		}
		private void StockManagementButton_Click(object sender, RoutedEventArgs e)
		{
			// Navigate the main frame to the POSView
			MainFrame.Navigate(new StockManagementView());
		}
	}
}
