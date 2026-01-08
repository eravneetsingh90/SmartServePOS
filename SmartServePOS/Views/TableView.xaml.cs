using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	public partial class TableView : Page
	{
		public TableView()
		{
			InitializeComponent();

			//if (DataContext == null)
			//{
			//	if (App.Services is not null)
			//	{
			//		DataContext = App.Services.GetService<TableViewModel>();
			//	}
			//}
		}
	}
}
