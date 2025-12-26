using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class CategoryViewModel : BaseViewModel
	{
		private readonly ICategoryStore _categoryStore;
		private readonly INotificationService _notificationService;
		private readonly IDialogService _dialogService;
		public ObservableCollection<Category> Categories { get; } = new();

		public ICommand AddCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand RefreshCommand { get; }
		public ICommand DeleteCommand { get; }

		public CategoryViewModel(ICategoryStore categoryStore, INotificationService notificationService, IDialogService dialogService)
		{
			_categoryStore = categoryStore;
			_notificationService = notificationService;
			_dialogService = dialogService;
			AddCommand = new RelayCommand(_ => AddCategory());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			RefreshCommand = new RelayCommand(async _ => await LoadAsync());
			DeleteCommand = new RelayCommand(DeleteCategory);

			_ = LoadAsync();
		}

		private async Task LoadAsync()
		{
			Categories.Clear();
			var data = await _categoryStore.GetAllCategoriesByOrderAsync();
			foreach (var c in data)
				Categories.Add(c);
		}

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

		private async Task SaveAsync()
		{
			try
			{
				await _categoryStore.SaveBulkCategoriesAsync(Categories);
				_notificationService.Success("Categories saved successfully");
			}
			catch (Exception ex)
			{
				await _dialogService.ShowWarningAsync(string.Empty,ex.Message);
				return;
			}
		}

		private async void DeleteCategory(object? parameter)
		{
			if (parameter is not Category category)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete category \"{category.Name}\"?");

			if (!result)
				return;

			Categories.Remove(category);

			if (category.CategoryId != 0)
				await _categoryStore.DeleteAndSaveAsync(category);
		}
	}
}
