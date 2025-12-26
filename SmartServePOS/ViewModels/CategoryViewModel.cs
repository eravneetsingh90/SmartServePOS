using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;

namespace SmartServePOS.ViewModels
{
	public class CategoryViewModel : BaseViewModel
	{
		private readonly ICategoryStore _categoryStore;

		public ObservableCollection<Category> Categories { get; } = new();

		public ICommand AddCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand RefreshCommand { get; }
		public ICommand DeleteCommand { get; }

		public CategoryViewModel(ICategoryStore categoryStore)
		{
			_categoryStore = categoryStore;

			AddCommand = new RelayCommand(_ => AddCategory());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			RefreshCommand = new RelayCommand(async _ => await LoadAsync());
			DeleteCommand = new RelayCommand(DeleteCategory);

			_ = LoadAsync();
		}

		// ================= LOAD =================
		private async Task LoadAsync()
		{
			Categories.Clear();
			var data = await _categoryStore.GetAllCategoriesByOrderAsync();
			foreach (var c in data)
				Categories.Add(c);
		}

		// ================= ADD =================
		private void AddCategory()
		{
			int nextOrder = Categories.Any()
			? Categories.Max(c => c.DisplayOrder) + 1
			: 1;

			Categories.Add(new Category
			{
				Name = "New Category",
				IsActive = true,
				DisplayOrder = nextOrder
			});
		}

		// ================= SAVE =================
		private async Task SaveAsync()
		{
			try
			{
				await _categoryStore.SaveBulkCategoriesAsync(Categories);
				MessageBox.Show("Success", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				//await LoadAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					ex.Message,
					String.Empty,
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
				return;
			}
		}

		// ================= DELETE WITH CONFIRM =================
		private async void DeleteCategory(object? parameter)
		{
			if (parameter is not Category category)
				return;

			var result = MessageBox.Show(
				$"Are you sure you want to delete category \"{category.Name}\"?",
				"Confirm Delete",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			if (result != MessageBoxResult.Yes)
				return;

			Categories.Remove(category);

			if (category.CategoryId != 0)
				await _categoryStore.DeleteAndSaveAsync(category);
		}
	}
}
