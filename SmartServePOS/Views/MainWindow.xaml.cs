using SmartServe.Domain.Stores;
using System.Windows;

namespace SmartServePOS.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow(ProductStore productStore)
		{
			InitializeComponent();
		}
	}
}
