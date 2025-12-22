using System.Collections.ObjectModel;
using System.Windows.Input;
using SmartServePOS.Command;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;

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

			// Load tracked entities (important for edit + save)
			var data = await _categoryStore.GetAllAsync(asNoTracking: false);

			foreach (var category in data)
				Categories.Add(category);
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

		// ================= SAVE (BATCH) =================
		private async Task SaveAsync()
		{
			/*
             * Strategy:
             * - New items → Add
             * - Existing items → Update
             * - Single SaveChanges at end
             */

			foreach (var category in Categories)
			{
				if (category.CategoryId == 0)
					await _categoryStore.AddAsync(category);
				else
					await _categoryStore.UpdateAsync(category);
			}

			// Optional reload to sync state
			await LoadAsync();
		}

		// ================= DELETE =================
		private async void DeleteCategory(object? parameter)
		{
			if (parameter is not Category category)
				return;

			Categories.Remove(category);

			// Only delete if it already exists in DB
			if (category.CategoryId != 0)
				await _categoryStore.DeleteAsync(category);
		}
	}
}
