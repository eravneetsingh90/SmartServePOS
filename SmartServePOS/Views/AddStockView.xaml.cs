using SmartServe.Domain.Models;
using SmartServePOS.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

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
		}
		private void VariantClicked(object sender, MouseButtonEventArgs e)
		{
			if (sender is ListBoxItem item &&
				item.DataContext is ProductVariantDto variant &&
				DataContext is AddStockViewModel vm &&
				vm.AddVariantCommand.CanExecute(variant))
			{
				vm.AddVariantCommand.Execute(variant);
			}
		}
	}
}
