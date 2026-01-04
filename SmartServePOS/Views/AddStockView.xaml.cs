using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	/// <summary>
	/// Interaction logic for AddStockView.xaml
	/// </summary>
	public partial class AddStockView : Page
	{
		public AddStockView()
		{
			InitializeComponent();
			if (DataContext == null)
			{
				if (App.Services is not null)
				{
					DataContext = App.Services.GetService<AddStockViewModel>();
				}
			}
		}
	}
}
