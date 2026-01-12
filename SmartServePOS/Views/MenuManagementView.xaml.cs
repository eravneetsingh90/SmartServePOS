using SmartServePOS.Helper;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	public partial class MenuManagementView : Page
	{
		private readonly INavigationService _navigationService;
		public MenuManagementView(INavigationService navigationService)
		{
			InitializeComponent();
			_navigationService = navigationService;
		}

		private void OpenCategories_Click(object sender, MouseButtonEventArgs e)
		{
			_navigationService.NavigateToCategoryView();
		}

		private void OpenProducts_Click(object sender, MouseButtonEventArgs e)
		{
			_navigationService.NavigateToProductView();
		}

		private void OpenVariants_Click(object sender, MouseButtonEventArgs e)
		{
			_navigationService.NavigateToProductVariantView();
			
		}
	}
}
