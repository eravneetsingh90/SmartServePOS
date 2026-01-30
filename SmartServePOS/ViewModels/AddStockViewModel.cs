using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Constant;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SmartServePOS.ViewModels
{
	public class AddStockViewModel : BaseViewModel
	{
		#region fields
		private readonly IMapper _mapper;
		private readonly INavigationService _navigationService;
		private readonly INotificationService _notificationService;
		private string _searchText;
		private bool _isSearchActive;
		private readonly IProductService _productService;
		private readonly IStockService _stockService;
		private List<Stock> _stocks = new();
		public ObservableCollection<Category> Categories { get; } = new();
		public ObservableCollection<Product> Products { get; }
		public ObservableCollection<ProductVariant> Variants { get; } = new();
		public ObservableCollection<AddStockModel> StockRows { get; } = new();
		#endregion

		#region Selected Items (Variant Flow)

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
			INotificationService notificationService,
			IProductService productService,
			IStockService stockService,
			INavigationService navigationService)
		{
			_mapper = mapper;
			_notificationService = notificationService;
			_productService = productService;
			_stockService = stockService;
			Categories = new ObservableCollection<Category>();
			Products = new ObservableCollection<Product>();
			Variants = new ObservableCollection<ProductVariant>();
			IncreaseQtyCommand = new RelayCommand<AddStockModel>(IncreaseQty);
			DecreaseQtyCommand = new RelayCommand<AddStockModel>(DecreaseQty);
			AddVariantCommand = new RelayCommand<ProductVariant>(AddVariant);
			RemoveRowCommand = new RelayCommand<AddStockModel>(RemoveRow);
			SaveStockCommand = new RelayCommand(async _ => await SaveStockAsync());
			ReloadCommand = new RelayCommand(async _ => await ReloadAsync());
			ClearCommand = new RelayCommand(ClearStockRows);
			_navigationService = navigationService;
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
								.GroupBy(s => s.Variant.Product.Category.Id)
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
								.Where(s => s.Variant?.Product != null && s.Variant?.Product.CategoryId == SelectedCategory.Id)
								.GroupBy(s => s.Variant.Product.Id)
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

			foreach (var v in _stocks.Where(a=>a.Variant.ProductId==SelectedProduct.Id))
			{
				Variants.Add(v.Variant);
			}
		}
		private async Task ReloadAsync()
		{
			Variants.Clear();
			StockRows.Clear();

			await InitializeAsync();
		}

		private void AddVariant(ProductVariant variant)
		{
			var existing = StockRows.FirstOrDefault(x => x.VariantId == variant.Id);

			if (existing != null)
			{
				existing.Quantity++;
			}
			else
			{
				var currentstock = _stocks.FirstOrDefault(x => x.VariantId == variant.Id);
				StockRows.Add(new AddStockModel
				{
					ItemType = currentstock.ItemType,
					VariantId = variant.Id,
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

			var stocks = _mapper.Map<List<AddStock>>(StockRows.ToList());
			var response = await _stockService.AddStockAsync(stocks);
			if (response.MetaData.ResultCode == ResultCodes.Success)
			{
				StockRows.Clear();
				_notificationService.Success(UIConstants.SavedSuccessfully);
				_navigationService.NavigateToStockManagementView();
			}
			else
				_notificationService.Error(UIConstants.Error);
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
				Variants.Add(new ProductVariant
				{
					Id = item.VariantId,
					//ProductId = item.ProductId,
					VariantName = item.DisplayName,
					//Price = item.Price
				});
			}
		}

		#endregion
	}
}
