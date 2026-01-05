using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	/// <summary>
	/// Interaction logic for LinkInventory.xaml
	/// </summary>
	public partial class LinkInventoryView : Page
	{
		public LinkInventoryView()
		{
			InitializeComponent();
			if (DataContext == null)
			{
				if (App.Services is not null)
				{
					DataContext = App.Services.GetService<LinkInventoryViewModel>();
				}
			}
		}
	}
}
