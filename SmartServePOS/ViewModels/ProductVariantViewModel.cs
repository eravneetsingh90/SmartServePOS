using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Constant;
using SmartServePOS.Helper;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductVariantViewModel : BaseViewModel
	{
		#region services
		private readonly IProductService _productService;
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
		public ObservableCollection<CategoryDto> Categories { get; }
		public ObservableCollection<ProductDto> Products { get; }
		public ObservableCollection<ProductVariantDto> Variants { get; }
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

		#region constructor
		public ProductVariantViewModel(
			IMapper mapper,
			INotificationService notificationService,
			IDialogService dialogService,
			IProductService productService)
		{
			_mapper = mapper;
			_notificationService = notificationService;
			_dialogService = dialogService;
			_productService = productService;
			Categories = new ObservableCollection<CategoryDto>();
			Products = new ObservableCollection<ProductDto>();
			Variants = new ObservableCollection<ProductVariantDto>();
			
			AddVariantCommand = new RelayCommand(_ => AddVariant());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteCommand = new RelayCommand<ProductVariantDto>(DeleteVariant);
			RefreshCommand = new RelayCommand(async _ => await LoadVariantsAsync());
			_ = LoadCategoriesAsync();
			
		}
		#endregion

		#region methods
		private async Task LoadCategoriesAsync()
		{
			Categories.Clear();
			var items = await _productService.GetCategoriesAsync();
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

			var products = await _productService.GetProductByCategoryIdAsync(SelectedCategory.CategoryId);
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

		private void AddVariant()
		{
			if (SelectedProduct == null)
				return;

			var nextOrder = Variants.Any()
				? Variants.Max(x => x.DisplayOrder) + 1
				: 1;

			var variant = new ProductVariantDto
			{
				ProductId = SelectedProduct.ProductId,
				VariantName = "New Variant",
				Price = 0,
				IsActive = true,
				DisplayOrder = nextOrder
			};

			Variants.Add(variant);
			OnPropertyChanged(nameof(Variants));
		}

		private async void DeleteVariant(ProductVariantDto? variant)
		{
			if (variant == null)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete variant \"{variant.VariantName}\"?");

			if (!result)
				return;

			if (variant.VariantId != 0)
			{
				var response = await _productService.DeleteVariantAsync(variant.VariantId);
				if (response.MetaData.ResultCode == ResultCodes.Success)
				{
					Variants.Remove(variant);
					_notificationService.Success(UIConstants.DeletedSuccessfully);
				}
				else
					_notificationService.Error(UIConstants.Error);
			}
		}

		private async Task SaveAsync()
		{
			try
			{
				var productVariants = _mapper.Map<List<ProductVariantDto>>(Variants);
				await _productService.SaveBulkVariantAsync(productVariants);
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
