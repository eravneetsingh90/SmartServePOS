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
		private string _searchText;
		private bool _isSearchActive;
		private readonly IProductService _productService;
		private readonly IStockService _stockService;
		private List<StockDto> _stocks = new();
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
		public string SearchText
		{
			get => _searchText;
			set
			{
				if (_searchText == value) return;

				_searchText = value;
				OnPropertyChanged(nameof(SearchText));

				PerformSearch();
			}
		}
		public bool IsSearchActive
		{
			get => _isSearchActive;
			private set
			{
				_isSearchActive = value;
				OnPropertyChanged(nameof(IsSearchActive));
			}
		}

		#endregion

		#region Commands
		public ICommand IncreaseQtyCommand { get; }
		public ICommand DecreaseQtyCommand { get; }
		public ICommand AddVariantCommand { get; }
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
			IncreaseQtyCommand = new RelayCommand<AddStockModel>(IncreaseQty);
			DecreaseQtyCommand = new RelayCommand<AddStockModel>(DecreaseQty);
			AddVariantCommand = new RelayCommand<ProductVariantDto>(AddVariant);
			RemoveRowCommand = new RelayCommand<AddStockModel>(RemoveRow);
			SaveStockCommand = new RelayCommand(async _ => await SaveStockAsync());
			ReloadCommand = new RelayCommand(async _ => await ReloadAsync());
			ClearCommand = new RelayCommand(ClearStockRows);
		}

		#region Methods

		public async Task InitializeAsync()
		{
			//await LoadCategoriesAsync();
			_stocks = (await _productService.GetAllStockAsync()).ToList();
			await LoadCategoriesAsync();
		}
		private async Task LoadCategoriesAsync()
		{
			var uniqueCategories = _stocks
								.Where(s => s.Variant?.Product?.Category != null)
								.GroupBy(s => s.Variant.Product.Category.CategoryId)
								.Select(g => g.First().Variant.Product.Category);

			Categories.Clear();
			foreach (var category in uniqueCategories)
			{
				Categories.Add(category);
			}
			if (SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();
		}
		private async Task LoadProductsAsync()
		{
			var uniqueProducts = _stocks
								.Where(s => s.Variant?.Product != null && s.Variant?.Product.CategoryId == SelectedCategory.CategoryId)
								.GroupBy(s => s.Variant.Product.ProductId)
								.Select(g => g.First().Variant.Product);
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			foreach (var product in uniqueProducts)
			{
				Products.Add(product);
			}
			if (SelectedProduct == null)
				SelectedProduct = Products.FirstOrDefault();
		}

		private async Task LoadVariantsAsync()
		{
			Variants.Clear();

			if (SelectedProduct == null)
				return;

			foreach (var v in _stocks.Where(a=>a.Variant.ProductId==SelectedProduct.ProductId))
			{
				Variants.Add(v.Variant);
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

		private void AddVariant(ProductVariantDto variant)
		{
			var existing = StockRows.FirstOrDefault(x => x.VariantId == variant.VariantId);

			if (existing != null)
			{
				existing.Quantity++;
			}
			else
			{
				StockRows.Add(new AddStockModel
				{
					VariantId = variant.VariantId,
					DisplayName = $"{variant.Product.Name} - {variant.VariantName}",
					SearchText = (variant.Product.Category.Name + " " + variant.Product.Name + " " + variant.VariantName).ToLower(),
					Unit = "PCS",
					Quantity = 1,
					Reason = "PURCHASE"
				});
			}

		}

		private void IncreaseQty(AddStockModel item)
		{
			if (item == null) return;

			item.Quantity++;
		}
		private void DecreaseQty(AddStockModel item)
		{
			if (item == null) return;

			if (item.Quantity > 1)
			{
				item.Quantity--;
			}
			else
			{
				StockRows.Remove(item);
			}
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

		private void PerformSearch()
		{
			Variants.Clear();

			if (string.IsNullOrWhiteSpace(SearchText))
			{
				IsSearchActive = false;

				// restore normal flow
				if (SelectedProduct != null)
					_= LoadVariantsAsync();

				return;
			}

			IsSearchActive = true;

			var term = SearchText.Trim().ToLower();

			var results = StockRows
				.Where(x => x.SearchText.Contains(term))
				.Take(30)
				.ToList();

			foreach (var item in results)
			{
				Variants.Add(new ProductVariantDto
				{
					VariantId = item.VariantId,
					//ProductId = item.ProductId,
					VariantName = item.DisplayName,
					//Price = item.Price
				});
			}
		}

		#endregion
	}
}
