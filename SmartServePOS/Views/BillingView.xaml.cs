using SmartServe.Domain.Models;
using SmartServePOS.Models;
using SmartServePOS.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.Views
{
	public partial class BillingView : Page
	{
		public BillingView()
		{
			InitializeComponent();
		}
		private void VariantClicked(object sender, MouseButtonEventArgs e)
		{
			if (sender is ListBoxItem item &&
				item.DataContext is ProductVariant variant &&
				DataContext is BillingViewModel vm &&
				vm.AddVariantCommand.CanExecute(variant))
			{
				vm.AddVariantCommand.Execute(variant);
			}
		}

	}
}
