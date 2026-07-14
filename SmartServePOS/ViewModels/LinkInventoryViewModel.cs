using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Interfaces;
using SmartServe.Domain.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class LinkInventoryViewModel : BaseViewModel
	{
		#region commands
		public ICommand ToggleStockCommand { get; }
		#endregion
		#region Fields
		private readonly IMapper _mapper;
		private readonly IProductService _productService;
		private readonly INotificationService _notificationService;
		private readonly IStockService _stockService;

		public ObservableCollection<Category> Categories { get; } = new();
		public ObservableCollection<Product> Products { get; } = new();
		public ObservableCollection<LinkInventoryDto> Variants { get; } = new();

		public List<Option> ItemTypes { get; } = new()
		{
			new Option { Value = null, Display = "-- Select --" },
			new Option { Value = StockItemType.VARIANT, Display = StockItemType.VARIANT },
			new Option { Value = StockItemType.INGREDIENT, Display = StockItemType.INGREDIENT }
		};
		public List<Option> UnitTypes { get; } = new()
		{
			new Option { Value = null, Display = "-- Select --" },
			new Option { Value = UnitType.PCS, Display = UnitType.PCS},
			new Option { Value = UnitType.ML, Display = UnitType.ML},
			new Option { Value = UnitType.GM, Display = UnitType.GM}
		};
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

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set => SetProperty(ref _isLoading, value);
		}



		#endregion

		#region Constructor

		public LinkInventoryViewModel(
			IProductService productService,
			IStockService stockService,
			IMapper mapper,
			INotificationService notificationService)
		{
			_mapper = mapper;
			_notificationService = notificationService;
			_productService = productService;
			_stockService = stockService;

			ToggleStockCommand =
				new RelayCommand<LinkInventoryDto>(ToggleVariantStockAsync);

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

		#region Loading Logic

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

			SelectedCategory = Categories.FirstOrDefault();
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

			// Load ALL stock items (variant + ingredient)
			var stockItems = await _stockService.GetStockAsync();
			var stockLookup = stockItems.ToDictionary(x => x.VariantId);

			foreach (var variant in variants)
			{
				var model = new LinkInventoryDto
				{
					VariantId = variant.Id,
					VariantName = variant.VariantName,
					Price = variant.Price,
					ProductName = SelectedProduct.Name,
					CategoryName = SelectedCategory?.Name??string.Empty,
					IsStockTracked = false,
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				};

				if (stockLookup.TryGetValue(variant.Id, out var stock))
				{
					model.Id = stock.Id;
					model.ItemType = stock.ItemType;
					model.Unit = stock.Unit;
					model.MinStockLevel = stock.MinStockLevel??0;
					model.IsStockTracked = Convert.ToBoolean(stock.IsActive);
					model.IsActive = !Convert.ToBoolean(stock.IsActive);
					model.CreatedAt = stock.CreatedAt ?? DateTime.UtcNow;
				}
				
				Variants.Add(model);
			}

			IsLoading = false;
		}

		#endregion

		#region Stock Toggle

		private async void ToggleVariantStockAsync(LinkInventoryDto model)
		{
			if (model == null)
				return;
			if(model.ItemType == null)
			{
				_notificationService.Error("Please select Item Type before adding to stock.");
				return;
			}
			if(model.Unit == null)
			{
				_notificationService.Error("Please select Unit before adding to stock.");
				return;
			}
			IsLoading = true;
			var stock = _mapper.Map<Stock>(model);
			await _stockService.ActivateStockItemAsync(stock);
			_ = LoadVariantsAsync();

			IsLoading = false;
		}

		#endregion

	}
}
