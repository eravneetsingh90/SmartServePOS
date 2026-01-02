using AutoMapper;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductVariantViewModel : BaseViewModel
	{
		#region stores
		private readonly ICategoryStore _categoryStore;
		private readonly IProductStore _productStore;
		private readonly IProductVariantStore _variantStore;
		private readonly ICatalogService _catalogService;
		#endregion

		#region services
		private readonly INotificationService _notificationService;
		private readonly IDialogService _dialogService;
		#endregion

		#region commands
		public ICommand RefreshCommand { get; }
		public ICommand AddVariantCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand DeleteCommand { get; }
		#endregion

		#region properties
		private readonly IMapper _mapper;
		public ObservableCollection<Category> Categories { get; }
		public ObservableCollection<Product> Products { get; }
		public ObservableCollection<ProductVariantModel> Variants { get; }
		public IEnumerable<StockMode> StockModes { get; } = Enum.GetValues(typeof(StockMode)).Cast<StockMode>();
		public IEnumerable<Brand> Brands { get; }
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
		#endregion

		#region constructor
		public ProductVariantViewModel(
			IMapper mapper,
		ICategoryStore categoryStore,
			IProductStore productStore,
			IProductVariantStore variantStore,
			INotificationService notificationService,
			IDialogService dialogService,
			ICatalogService catalogService)
		{
			_mapper = mapper;
			_categoryStore = categoryStore;
			_productStore = productStore;
			_variantStore = variantStore;
			_notificationService = notificationService;
			_dialogService = dialogService;
			_catalogService = catalogService;

			Categories = new ObservableCollection<Category>();
			Products = new ObservableCollection<Product>();
			Variants = new ObservableCollection<ProductVariantModel>();

			Brands = _catalogService.GetBrands();
			AddVariantCommand = new RelayCommand(_ => AddVariant());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteCommand = new RelayCommand<ProductVariantModel>(DeleteVariant);
			RefreshCommand = new RelayCommand(async _ => await LoadVariantsAsync());
			_ = LoadCategoriesAsync();
		}
		#endregion

		#region methods
		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();
			var items = await _categoryStore.GetAllAsync();

			foreach (var c in items)
				Categories.Add(c);
			if (SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();
		}

		private async Task LoadProductsAsync()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = await _productStore.GetByCategoryIdAsync(SelectedCategory.CategoryId);
			foreach (var p in products)
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

			var variants = await _variantStore.GetByProductIdAsync(SelectedProduct.ProductId);
			var variantModels = _mapper.Map<List<ProductVariantModel>>(variants);
			foreach (var v in variantModels)
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

			var variant = new ProductVariantModel
			{
				ProductId = SelectedProduct.ProductId,
				Name = "New Variant",
				Price = 0,
				IsActive = true,
				DisplayOrder = nextOrder,
				StockMode = StockMode.NONE
			};

			Variants.Add(variant);
			OnPropertyChanged(nameof(Variants));
		}

		private async void DeleteVariant(ProductVariantModel? variant)
		{
			if (variant == null)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete variant \"{variant.Name}\"?");

			if (!result)
				return;

			Variants.Remove(variant);

			if (variant.ProductVariantId != 0)
				_ = _variantStore.DeleteAsync(variant.ProductVariantId);
		}

		private async Task SaveAsync()
		{
			try
			{
				var productVariants = _mapper.Map<List<ProductVariant>>(Variants);
				await _variantStore.SaveBulkAsync(productVariants);
				_notificationService.Success("Saved Successfully");
				await LoadVariantsAsync();
			}
			catch (Exception ex)
			{
				await _dialogService.ShowWarningAsync(string.Empty, ex.Message);
				return;
			}
		}
		#endregion
	}
}
