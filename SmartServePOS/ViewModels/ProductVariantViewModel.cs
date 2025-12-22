using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductVariantViewModel : BaseViewModel
	{
		private readonly CategoryStore _categoryStore;
		private readonly ProductStore _productStore;
		private readonly ProductVariantStore _variantStore;

		public ProductVariantViewModel(
			CategoryStore categoryStore,
			ProductStore productStore,
			ProductVariantStore variantStore)
		{
			_categoryStore = categoryStore;
			_productStore = productStore;
			_variantStore = variantStore;

			Categories = new ObservableCollection<Category>();
			Products = new ObservableCollection<Product>();
			Variants = new ObservableCollection<ProductVariant>();

			AddVariantCommand = new RelayCommand(_ => AddVariant());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteVariantCommand = new RelayCommand<ProductVariant>(DeleteVariant);

			_ = LoadCategoriesAsync();
		}

		// =========================
		// COLLECTIONS
		// =========================
		public ObservableCollection<Category> Categories { get; }
		public ObservableCollection<Product> Products { get; }
		public ObservableCollection<ProductVariant> Variants { get; }

		// =========================
		// SELECTED CATEGORY
		// =========================
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

		// =========================
		// SELECTED PRODUCT
		// =========================
		private Product? _selectedProduct;
		public Product? SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				_selectedProduct = value;
				OnPropertyChanged();
				_ = LoadVariantsAsync();
			}
		}

		// =========================
		// COMMANDS
		// =========================
		public ICommand AddVariantCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand DeleteVariantCommand { get; }

		// =========================
		// LOAD DATA
		// =========================
		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();
			var items = await _categoryStore.GetAllAsync();

			foreach (var c in items.Where(x => x.IsActive==true))
				Categories.Add(c);
		}

		private async Task LoadProductsAsync()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = await _productStore.GetAllAsync();
			foreach (var p in products
				.Where(x => x.CategoryId == SelectedCategory.CategoryId && x.IsActive==true))
			{
				Products.Add(p);
			}
		}

		private async Task LoadVariantsAsync()
		{
			Variants.Clear();

			if (SelectedProduct == null)
				return;

			var variants = await _variantStore.GetAllAsync();
			foreach (var v in variants
				.Where(x => x.ProductId == SelectedProduct.ProductId))
			{
				Variants.Add(v);
			}
		}

		// =========================
		// ADD VARIANT
		// =========================
		private void AddVariant()
		{
			if (SelectedProduct == null)
				return;

			var nextOrder = Variants.Any()
				? Variants.Max(x => x.DisplayOrder) + 1
				: 1;

			var variant = new ProductVariant
			{
				ProductId = SelectedProduct.ProductId,
				Name = "New Variant",
				Price = 0,
				IsActive = true,
				DisplayOrder = nextOrder
			};

			Variants.Add(variant);
			OnPropertyChanged(nameof(Variants));
		}

		private bool CanAddVariant()
		{
			return SelectedProduct != null;
		}

		// =========================
		// DELETE VARIANT
		// =========================
		private void DeleteVariant(ProductVariant? variant)
		{
			if (variant == null)
				return;

			var result = MessageBox.Show(
				$"Delete variant '{variant.Name}'?",
				"Confirm Delete",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			if (result != MessageBoxResult.Yes)
				return;

			Variants.Remove(variant);

			if (variant.ProductVariantId != 0)
				_ = _variantStore.DeleteAsync(variant);
		}

		// =========================
		// SAVE
		// =========================
		private async Task SaveAsync()
		{
			// Simple duplicate check per product
			var duplicate = Variants
				.GroupBy(x => x.Name.Trim().ToLower())
				.Any(g => g.Count() > 1);

			if (duplicate)
			{
				MessageBox.Show(
					"Duplicate variant names are not allowed for the same product.",
					"Validation Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}

			foreach (var variant in Variants)
			{
				if (variant.ProductVariantId == 0)
					await _variantStore.AddAsync(variant);
				else
					await _variantStore.UpdateAsync(variant);
			}

			await LoadVariantsAsync();
		}

		private bool CanSave()
		{
			return Variants.Any();
		}
	}
}
