using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SmartServePOS.Views
{
	public partial class CategoryView : Page
	{
		public ObservableCollection<CategoryDto> Categories { get; } = new();

		public CategoryView()
		{
			InitializeComponent();
			CategoryGrid.ItemsSource = Categories;

			// TEMP DATA
			Categories.Add(new CategoryDto { Name = "Ice Cream Scoops", IsActive = true });
			Categories.Add(new CategoryDto { Name = "Burgers", IsActive = true });
			Categories.Add(new CategoryDto { Name = "Beverages", IsActive = false });
		}

		private void AddCategory_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Categories.Add(new CategoryDto
			{
				Name = "New Category",
				IsActive = true
			});
		}

		private void SaveChanges_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// Later: validate + call service
			// For now: no-op
		}
	}
}
