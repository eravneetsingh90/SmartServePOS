using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class LinkInventoryViewModel : BaseViewModel
	{
		#region fields
		private readonly ICatalogService _catalogService;
		private readonly IStockService _stockService;

		public ObservableCollection<CategoryDto> Categories { get; } = new();
		public ObservableCollection<ProductDto> Products { get; } = new();
		public ObservableCollection<LinkInventoryModel> Variants { get; } = new();

		private CategoryDto _selectedCategory;
		public CategoryDto SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				SetProperty(ref _selectedCategory, value);
				LoadProducts();
			}
		}

		private ProductDto _selectedProduct;
		public ProductDto SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				SetProperty(ref _selectedProduct, value);
				LoadVariants();
			}
		}

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		public ICommand ToggleStockCommand { get; }
		#endregion
		public LinkInventoryViewModel(
			ICatalogService catalogService,
			IStockService stockService)
		{
			_catalogService = catalogService;
			_stockService = stockService;

			//ToggleStockCommand = new RelayCommand<LinkInventoryModel>(
				//async v => await ToggleStockAsync(v));

			LoadCategories();
		}

		private void LoadCategories()
		{
			IsLoading = true;

			Categories.Clear();
			Products.Clear();
			Variants.Clear();

			foreach (var category in _catalogService.GetCategories())
			{
				Categories.Add(new CategoryDto
				{
					CategoryId = category.CategoryId,
					Name = category.Name
				});
			}
			if (SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();

			IsLoading = false;

		}
		private void LoadProducts()
		{
			if (SelectedCategory == null)
				return;

			var products = _catalogService.GetProductsByCategory(SelectedCategory.CategoryId);

			foreach (var product in products)
			{
				Products.Add(new ProductDto
				{
					CategoryId = product.CategoryId ?? 0,
					ProductId = product.ProductId,
					Name = product.Name
				});
			}
			if (SelectedProduct == null)
				SelectedProduct = Products.FirstOrDefault();
		}
		private void LoadVariants()
		{
			if (SelectedProduct == null)
				return;
			IsLoading = true;
			Variants.Clear();
			var variants = _catalogService.GetVariantsByProduct(SelectedProduct.ProductId);

			foreach (var variant in variants)
			{
				Variants.Add(new LinkInventoryModel
				{
					VariantId = variant.VariantId,
					Price = variant.Price,
					VariantName = variant.VariantName
				});
			}
		}
	}
}
