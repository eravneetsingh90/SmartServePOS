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
		public MainWindow(MainWindowViewModel vm)
		{
			InitializeComponent();
			DataContext = vm;
		}

		// Navigate to a Page instance resolved from DI
		public void Navigate(Page page)
		{
			MainFrame.Navigate(page);
		}

		private void NewOrderButton_Click(object sender, RoutedEventArgs e)
		{
			// Navigate the main frame to the POSView
			MainFrame.Navigate(new TableView());
		}

		private void MenuManagementButton_Click(object sender, RoutedEventArgs e)
		{
			// Navigate the main frame to the POSView
			MainFrame.Navigate(new MenuManagementView());
		}
	}
}
