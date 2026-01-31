using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	/// <summary>
	/// Interaction logic for StockView.xaml
	/// </summary>
	public partial class CurrentStockView : Page
	{
		public CurrentStockView()
		{
			InitializeComponent();
			//if (DataContext == null)
			//{
			//	if (App.Services is not null)
			//	{
			//		DataContext = App.Services.GetService<CurrentStockViewModel>();
			//	}
			//}
		}
	}
}
