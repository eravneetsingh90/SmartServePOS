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
		private readonly CategoryStore _categoryStore;

		public ObservableCollection<Category> Categories { get; } = new();

		public ICommand AddCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand DeleteCommand { get; }

		public CategoryViewModel(CategoryStore categoryStore)
		{
			_categoryStore = categoryStore;

			AddCommand = new RelayCommand(_ => AddCategory());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteCommand = new RelayCommand(DeleteCategory);

			_ = LoadAsync();
		}

		// ================= LOAD =================
		private async Task LoadAsync()
		{
			Categories.Clear();

			var data = await _categoryStore.GetAllAsync(asNoTracking: false);
			foreach (var c in data)
				Categories.Add(c);
		}

		// ================= ADD =================
		private void AddCategory()
		{
			Categories.Add(new Category
			{
				Name = "New Category",
				IsActive = true
			});
		}

		// ================= SAVE =================
		private async Task SaveAsync()
		{
			foreach (var category in Categories)
			{
				if (category.CategoryId == 0)
					await _categoryStore.AddAsync(category);
				else
					await _categoryStore.UpdateAsync(category);
			}

			await LoadAsync();
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
				await _categoryStore.DeleteAsync(category);
		}
	}
}
