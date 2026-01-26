using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class LinkInventoryViewModel : BaseViewModel
	{
		#region Fields

		private readonly IProductService _productService;
		private readonly IStockService _stockService;

		private readonly SemaphoreSlim _ingredientLock = new(1, 1);
		private CancellationTokenSource _ingredientCts;

		#endregion

		#region Collections

		public ObservableCollection<Category> Categories { get; } = new();
		public ObservableCollection<Product> Products { get; } = new();
		public ObservableCollection<LinkInventoryModel> Variants { get; } = new();
		public ObservableCollection<IngredientStockSetupModel> Ingredients { get; } = new();

		#endregion

		#region Properties

		private Category _selectedCategory;
		public Category SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				if (SetProperty(ref _selectedCategory, value))
					_ = LoadProductsAsync();
			}
		}

		private Product _selectedProduct;
		public Product SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				if (SetProperty(ref _selectedProduct, value))
					_ = LoadVariantsAsync();
			}
		}

		private string _ingredientSearchText;
		public string IngredientSearchText
		{
			get => _ingredientSearchText;
			set
			{
				if (SetProperty(ref _ingredientSearchText, value))
					DebounceIngredientLoad();
			}
		}

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}

		private bool _isIngredientLoading;
		public bool IsIngredientLoading
		{
			get => _isIngredientLoading;
			set => SetProperty(ref _isIngredientLoading, value);
		}

		private int _selectedTabIndex;
		public int SelectedTabIndex
		{
			get => _selectedTabIndex;
			set
			{
				if (SetProperty(ref _selectedTabIndex, value))
				{
					if (value == 1) // Ingredients tab index
					{
						_ = LoadIngredientsOnceAsync();
					}
				}
			}
		}

		private bool _ingredientsLoaded;

		private async Task LoadIngredientsOnceAsync()
		{
			if (_ingredientsLoaded)
				return;

			_ingredientsLoaded = true;
			await LoadIngredientsAsync();
		}

		#endregion

		#region Commands

		public ICommand ToggleStockCommand { get; }
		public ICommand ToggleIngredientStockCommand { get; }

		#endregion

		#region Constructor

		public LinkInventoryViewModel(
			IProductService productService,
			IStockService stockService)
		{
			_productService = productService;
			_stockService = stockService;

			ToggleStockCommand = new RelayCommand<LinkInventoryModel>(
				async v => await ToggleVariantStockAsync(v));

			ToggleIngredientStockCommand = new RelayCommand<IngredientStockSetupModel>(
				async i => await ToggleIngredientStockAsync(i));

			_ = InitializeAsync();
		}

		#endregion

		#region Initialization

		private async Task InitializeAsync()
		{
			IsLoading = true;

			await LoadCategoriesAsync();
			
			IsLoading = false;
		}

		#endregion

		#region Category / Product / Variant Loading

		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();

			var categories = await _productService.GetCategoriesAsync();

			foreach (var category in categories)
			{
				Categories.Add(new Category
				{
					Id = category.Id,
					Name = category.Name
				});
			}

			SelectedCategory ??= Categories.FirstOrDefault();
		}

		private async Task LoadProductsAsync()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = await _productService.GetProductByCategoryIdAsync(SelectedCategory.Id);

			foreach (var product in products)
			{
				Products.Add(new Product
				{
					Id = product.Id,
					CategoryId = product.CategoryId ?? 0,
					Name = product.Name
				});
			}

			SelectedProduct = Products.FirstOrDefault();
		}

		private async Task LoadVariantsAsync()
		{
			if (SelectedProduct == null)
				return;

			IsLoading = true;
			Variants.Clear();

			var variants = await _productService.GetVariantByProductIdAsync(SelectedProduct.Id);

			var stockItems = await _stockService.GetStockItemAsync(StockItem.VARIANT);
			var stockLookup = stockItems.ToDictionary(x => x.VariantId);

			foreach (var variant in variants)
			{
				Variants.Add(new LinkInventoryModel
				{
					VariantId = variant.Id,
					VariantName = variant.VariantName,
					Price = variant.Price,
					ProductName = SelectedProduct.Name,
					CategoryName = SelectedCategory?.Name,
					IsStockTracked = stockLookup.ContainsKey(variant.Id)
				});
			}

			IsLoading = false;
		}

		#endregion

		#region Ingredient Loading (Serialized + Debounced)

		private void DebounceIngredientLoad()
		{
			_ingredientCts?.Cancel();
			_ingredientCts = new CancellationTokenSource();

			_ = LoadIngredientsDebouncedAsync(_ingredientCts.Token);
		}

		private async Task LoadIngredientsDebouncedAsync(CancellationToken token)
		{
			try
			{
				await Task.Delay(300, token);
				await LoadIngredientsAsync();
			}
			catch (TaskCanceledException) { }
		}

		private async Task LoadIngredientsAsync()
		{
			await _ingredientLock.WaitAsync();
			try
			{
				IsIngredientLoading = true;
				Ingredients.Clear();

				var ingredients = await _stockService.GetIngredients();
				var stockItems = await _stockService.GetStockItemAsync(StockItem.INGREDIENT);

				var stockLookup = stockItems.ToDictionary(x => x.VariantId);

				foreach (var ing in ingredients)
				{
					if (!string.IsNullOrWhiteSpace(IngredientSearchText) &&
						!ing.Name.Contains(IngredientSearchText,
							StringComparison.OrdinalIgnoreCase))
						continue;

					Ingredients.Add(new IngredientStockSetupModel
					{
						IngredientId = ing.IngredientId,
						IngredientName = ing.Name,
						Unit = ing.Unit,
						IsStockTracked = stockLookup.ContainsKey(ing.IngredientId)
					});
				}
			}
			finally
			{
				IsIngredientLoading = false;
				_ingredientLock.Release();
			}
		}

		#endregion

		#region Toggle Stock

		private async Task ToggleVariantStockAsync(LinkInventoryModel variant)
		{
			if (variant == null)
				return;

			IsLoading = true;

			if (!variant.IsStockTracked)
			{
				await _stockService.ActivateStockItemAsync(
					StockItem.VARIANT,
					variant.VariantId);

				variant.IsStockTracked = true;
			}
			else
			{
				await _stockService.DeactivateStockItemAsync(
					StockItem.VARIANT,
					variant.VariantId);

				variant.IsStockTracked = false;
			}

			IsLoading = false;
		}

		private async Task ToggleIngredientStockAsync(IngredientStockSetupModel ingredient)
		{
			if (ingredient == null)
				return;

			IsIngredientLoading = true;

			if (!ingredient.IsStockTracked)
			{
				await _stockService.ActivateStockItemAsync(
					StockItem.INGREDIENT,
					ingredient.IngredientId);

				ingredient.IsStockTracked = true;
			}
			else
			{
				await _stockService.DeactivateStockItemAsync(
					StockItem.INGREDIENT,
					ingredient.IngredientId);

				ingredient.IsStockTracked = false;
			}

			IsIngredientLoading = false;
		}

		#endregion
	}
}
