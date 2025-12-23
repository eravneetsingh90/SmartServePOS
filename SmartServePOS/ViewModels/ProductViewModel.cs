using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductViewModel : BaseViewModel
	{
		private readonly CategoryStore _categoryStore;
		private readonly ProductStore _productStore;

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

		public ProductViewModel(
			CategoryStore categoryStore,
			ProductStore productStore)
		{
			_categoryStore = categoryStore;
			_productStore = productStore;

			AddProductCommand = new RelayCommand(_ => AddProduct());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteProductCommand = new RelayCommand(DeleteProduct);

			_ = LoadCategoriesAsync();
		}

		// =============================
		// LOAD CATEGORIES
		// =============================
		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();

			var data = await _categoryStore.GetAllAsync();

			foreach (var c in data.OrderBy(x => x.DisplayOrder))
				Categories.Add(c);

			SelectedCategory = Categories.FirstOrDefault();
		}

		// =============================
		// LOAD PRODUCTS BY CATEGORY
		// =============================
		private async Task LoadProductsAsync()
		{
			Products.Clear();

			if (SelectedCategory == null)
				return;

			var data = await _productStore.GetAllAsync();

			foreach (var p in data
				.Where(x => x.CategoryId == SelectedCategory.CategoryId)
				.OrderBy(x => x.DisplayOrder))
			{
				Products.Add(p);
			}
		}

		// =============================
		// ADD PRODUCT
		// =============================
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

		// =============================
		// SAVE (WITH DUPLICATE CHECK)
		// =============================
		private async Task SaveAsync()
		{
			if (SelectedCategory == null)
				return;

			// 🔴 Duplicate check (per category)
			var duplicateNames = Products
				.Where(p => !string.IsNullOrWhiteSpace(p.Name))
				.GroupBy(p => p.Name.Trim().ToLower())
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			if (duplicateNames.Any())
			{
				MessageBox.Show(
					"Duplicate product names are not allowed within the same category.",
					"Duplicate Products",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			// SAVE
			foreach (var product in Products)
			{
				if (product.ProductId == 0)
					await _productStore.AddAsync(product);
				else
					await _productStore.UpdateAsync(product);
			}

			await LoadProductsAsync();
		}

		// =============================
		// DELETE
		// =============================
		private async void DeleteProduct(object? parameter)
		{
			if (parameter is not Product product)
				return;

			var result = MessageBox.Show(
				$"Delete product \"{product.Name}\"?",
				"Confirm Delete",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			if (result != MessageBoxResult.Yes)
				return;

			Products.Remove(product);

			if (product.ProductId != 0)
				await _productStore.DeleteAsync(product);
		}
	}
}
