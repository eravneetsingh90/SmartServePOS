using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class AddStockViewModel : BaseViewModel
	{
		#region fields
		private readonly IMapper _mapper;
		private readonly IProductService _productService;
		private readonly IStockService _stockService;
		public ObservableCollection<CategoryDto> Categories { get; } = new();
		public ObservableCollection<ProductDto> Products { get; }
		public ObservableCollection<ProductVariantDto> Variants { get; } = new();
		public ObservableCollection<IngredientDto> Ingredients { get; } = new();
		public ObservableCollection<AddStockModel> StockRows { get; } = new();
		#endregion

		#region Selected Items (Variant Flow)

		private CategoryDto? _selectedCategory;
		public CategoryDto? SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				_selectedCategory = value;
				OnPropertyChanged();
				_ = LoadProductsAsync();
			}
		}
		private ProductDto? _selectedProduct;
		public ProductDto? SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				_selectedProduct = value;
				OnPropertyChanged();
				_ = LoadVariantsAsync();
			}
		}
		#endregion

		#region Commands
		public ICommand AddVariantCommand { get; }
		public ICommand AddIngredientCommand { get; }
		public ICommand RemoveRowCommand { get; }
		public ICommand SaveStockCommand { get; }
		public ICommand ReloadCommand { get; }
		public ICommand ClearCommand { get; }
		#endregion

		public AddStockViewModel(
			IMapper mapper,
			IProductService productService,
			IStockService stockService)
		{
			_mapper = mapper;
			_productService = productService;
			_stockService = stockService;
			Categories = new ObservableCollection<CategoryDto>();
			Products = new ObservableCollection<ProductDto>();
			Variants = new ObservableCollection<ProductVariantDto>();
			AddVariantCommand = new RelayCommand<ProductVariantDto>(AddVariant);
			AddIngredientCommand = new RelayCommand<IngredientDto>(AddIngredient);
			RemoveRowCommand = new RelayCommand<AddStockModel>(r => StockRows.Remove(r));
			SaveStockCommand = new RelayCommand(async _ => await SaveStockAsync());
			ReloadCommand = new RelayCommand(async _ => await ReloadAsync());
			ClearCommand = new RelayCommand(ClearStockRows);
		}

		#region Methods

		public async Task InitializeAsync()
		{
			//await LoadCategoriesAsync();
			var items = await _productService.GetAllStockAsync();

		}
		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();
			var items = await _productService.GetIsStockCategoriesAsync();
			var categoryModels = _mapper.Map<List<CategoryDto>>(items);
			foreach (var c in categoryModels)
			{
				Categories.Add(c);
			}
			if (SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();
		}
		private async Task LoadProductsAsync()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = await _productService.GetIsStockProductByCategoryIdAsync(SelectedCategory.CategoryId);
			var productModels = _mapper.Map<List<ProductDto>>(products);
			foreach (var p in productModels)
			{
				Products.Add(p);
			}
			if (SelectedProduct == null)
				SelectedProduct = Products.FirstOrDefault();
		}

		private async Task LoadVariantsAsync()
		{
			Variants.Clear();

			if (SelectedProduct == null)
				return;

			var variants = await _productService.GetVariantByProductIdAsync(SelectedProduct.ProductId);
			var variantModels = _mapper.Map<List<ProductVariantDto>>(variants);
			foreach (var v in variantModels)
			{
				Variants.Add(v);
			}
		}
		private async Task ReloadAsync()
		{
			Variants.Clear();
			Ingredients.Clear();
			StockRows.Clear();

			await InitializeAsync();
		}

		private async Task LoadIngredientsAsync()
		{
			var ingredients = await _stockService.GetIngredients();

			foreach (var ing in ingredients)
				Ingredients.Add(ing);
		}

		#endregion

		#region Add Stock Rows

		private void AddVariant(ProductVariantDto variant)
		{
			if (variant == null)
				return;

			if (StockRows.Any(x =>
				x.ItemType == StockItemType.VARIANT &&
				x.ReferenceId == variant.VariantId))
				return;

			StockRows.Add(new AddStockModel
			{
				ItemType = StockItemType.VARIANT,
				ReferenceId = variant.VariantId,
				DisplayName = $"{variant.Product.Name} - {variant.VariantName}",
				//Unit = variant.Unit ?? "PCS",
				Unit = "PCS",
				Quantity = 1,
				Reason = "PURCHASE"
			});
		}

		private void AddIngredient(IngredientDto ingredient)
		{
			if (ingredient == null)
				return;

			if (StockRows.Any(x =>
				x.ItemType == StockItemType.INGREDIENT &&
				x.ReferenceId == ingredient.IngredientId))
				return;

			StockRows.Add(new AddStockModel
			{
				ItemType = StockItemType.INGREDIENT,
				ReferenceId = ingredient.IngredientId,
				DisplayName = ingredient.Name,
				Unit = ingredient.Unit,
				Quantity = 1,
				Reason = "PURCHASE"
			});
		}

		private void RemoveRow(AddStockModel row)
		{
			if (row == null)
				return;

			StockRows.Remove(row);
		}

		private void ClearStockRows(object? obj)
		{
			StockRows.Clear();
		}

		#endregion

		#region Save

		private async Task SaveStockAsync()
		{
			if (!StockRows.Any())
				return;

			foreach (var row in StockRows)
			{
				if (row.Quantity <= 0)
					throw new InvalidOperationException("Quantity must be greater than zero.");
			}
			var stocks = _mapper.Map<List<AddStockDto>>(StockRows.ToList());
			await _stockService.AddStockAsync(stocks);

			StockRows.Clear();
		}

		#endregion
	}
}
