using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	public partial class MenuManagementView : Page
	{
		public MenuManagementView()
		{
			InitializeComponent();
		}

		private void OpenCategories_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new CategoryView());
		}

		private void OpenProducts_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new ProductView());
		}

		private void OpenVariants_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService?.Navigate(new ProductVariantView());
		}
	}
}
