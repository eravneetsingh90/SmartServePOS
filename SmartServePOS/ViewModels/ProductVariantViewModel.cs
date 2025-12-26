using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductVariantViewModel : BaseViewModel
	{
		private readonly ICategoryStore _categoryStore;
		private readonly IProductStore _productStore;
		private readonly IProductVariantStore _variantStore;
		private readonly INotificationService _notificationService;
		private readonly IDialogService _dialogService;
		public ICommand RefreshCommand { get; }
		public ProductVariantViewModel(
			ICategoryStore categoryStore,
			IProductStore productStore,
			IProductVariantStore variantStore,
			INotificationService notificationService,
			IDialogService dialogService)
		{
			_categoryStore = categoryStore;
			_productStore = productStore;
			_variantStore = variantStore;
			_notificationService = notificationService;
			_dialogService = dialogService;
			Categories = new ObservableCollection<Category>();
			Products = new ObservableCollection<Product>();
			Variants = new ObservableCollection<ProductVariant>();

			AddVariantCommand = new RelayCommand(_ => AddVariant());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteCommand = new RelayCommand<ProductVariant>(DeleteVariant);
			RefreshCommand = new RelayCommand(async _ => await LoadVariantsAsync());
			_ = LoadCategoriesAsync();
		}

		public ObservableCollection<Category> Categories { get; }
		public ObservableCollection<Product> Products { get; }
		public ObservableCollection<ProductVariant> Variants { get; }

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

		public ICommand AddVariantCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand DeleteCommand { get; }

		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();
			var items = await _categoryStore.GetAllCategoriesByOrderAsync();

			foreach (var c in items)
				Categories.Add(c);
			if(SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();
		}

		private async Task LoadProductsAsync()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = await _productStore.GetProductsByCategoryAsync(SelectedCategory.CategoryId);
			foreach (var p in products)
			{
				Products.Add(p);
			}
			if(SelectedProduct == null)
				SelectedProduct = Products.FirstOrDefault();
		}

		private async Task LoadVariantsAsync()
		{
			Variants.Clear();

			if (SelectedProduct == null)
				return;

			var variants = await _variantStore.GetProductsVariantByProductAsync(SelectedProduct.ProductId);
			foreach (var v in variants)
			{
				Variants.Add(v);
			}
		}

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

		private async void DeleteVariant(ProductVariant? variant)
		{
			if (variant == null)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete variant \"{variant.Name}\"?");

			if (!result)
				return;

			Variants.Remove(variant);

			if (variant.ProductVariantId != 0)
				_ = _variantStore.DeleteAndSaveAsync(variant);
		}

		private async Task SaveAsync()
		{
			try
			{
				await _variantStore.SaveBulkProductVariantsAsync(Variants);
				_notificationService.Success("Saved Successfully");
			}
			catch (Exception ex)
			{
				await _dialogService.ShowWarningAsync(string.Empty, ex.Message);
				return;
			}
		}
	}
}
