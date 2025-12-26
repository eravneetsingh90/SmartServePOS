using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductViewModel : BaseViewModel
	{
		private readonly ICategoryStore _categoryStore;
		private readonly IProductStore _productStore;
		private readonly INotificationService _notificationService;
		private readonly IDialogService _dialogService;
		public ObservableCollection<Category> Categories { get; } = new();
		public ObservableCollection<Product> Products { get; } = new();

		private Category? _selectedCategory;
		public Category? SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				_selectedCategory = value;
				OnPropertyChanged();
				_ = LoadProductsAsync();
			}
		}

		public ICommand AddProductCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand DeleteProductCommand { get; }
		public ICommand RefreshCommand { get; }
		public ProductViewModel(
			ICategoryStore categoryStore,
			IProductStore productStore,
			INotificationService notificationService, 
			IDialogService dialogService)
		{
			_categoryStore = categoryStore;
			_productStore = productStore;
			_notificationService = notificationService;
			_dialogService = dialogService;
			AddProductCommand = new RelayCommand(_ => AddProduct());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteProductCommand = new RelayCommand(DeleteProduct);
			RefreshCommand = new RelayCommand(async _ => await LoadProductsAsync());
			_ = LoadCategoriesAsync();
		}

		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();

			var data = await _categoryStore.GetAllCategoriesByOrderAsync();

			foreach (var c in data)
				Categories.Add(c);

			if(SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();
		}

		private async Task LoadProductsAsync()
		{
			Products.Clear();

			if (SelectedCategory == null)
				return;

			var data = await _productStore.GetProductsByCategoryAsync(SelectedCategory.CategoryId);
			foreach (var p in data)
			{
				Products.Add(p);
			}
		}

		private void AddProduct()
		{
			if (SelectedCategory == null)
				return;

			int nextOrder = Products.Any()
				? Products.Max(p => p.DisplayOrder) + 1
				: 1;

			Products.Add(new Product
			{
				CategoryId = SelectedCategory.CategoryId,
				Name = "New Product",
				IsActive = true,
				DisplayOrder = nextOrder
			});
		}

		private async Task SaveAsync()
		{
			if (SelectedCategory == null)
				return;
			try
			{
				await _productStore.SaveBulkProductsAsync(Products);
				_notificationService.Success("Products saved successfully");
			}
			catch (Exception ex)
			{
				await _dialogService.ShowWarningAsync(string.Empty, ex.Message);
				return;
			}
		}
		private async void DeleteProduct(object? parameter)
		{
			if (parameter is not Product product)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete product \"{product.Name}\"?");

			if (!result)
				return;

			Products.Remove(product);

			if (product.ProductId != 0)
				await _productStore.DeleteAndSaveAsync(product);
		}
	}
}
