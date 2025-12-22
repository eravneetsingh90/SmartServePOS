using Microsoft.Extensions.DependencyInjection;
using SmartServePOS.ViewModels;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	public partial class CategoryView : Page
	{
		public CategoryView()
		{
			InitializeComponent();
			if (DataContext == null)
			{
				if (App.Services is not null)
				{
					DataContext = App.Services.GetService<CategoryViewModel>();
				}
			}
		}
	}
}

