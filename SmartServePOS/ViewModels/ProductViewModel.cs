using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Constant;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class ProductViewModel : BaseViewModel
	{
		#region fields
		private readonly IMapper _mapper;
		private readonly IProductService _productService;
		private readonly INotificationService _notificationService;
		private readonly IDialogService _dialogService;
		public ObservableCollection<CategoryDto> Categories { get; } = new();
		public ObservableCollection<ProductModel> Products { get; } = new();

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

		public ICommand AddProductCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand DeleteProductCommand { get; }
		public ICommand RefreshCommand { get; }
		public List<FoodTypeOption> FoodTypeOptions { get; } = new()
		{
			new FoodTypeOption { Value = null, Display = "-- Select --" },
			new FoodTypeOption { Value = "VEG", Display = "Veg" },
			new FoodTypeOption { Value = "NON_VEG", Display = "Non-Veg" }
		};

		#endregion

		public ProductViewModel(
			IMapper mapper,
			IProductService productService,
			INotificationService notificationService,
			IDialogService dialogService)
		{
			_mapper = mapper;
			_productService = productService;
			_notificationService = notificationService;
			_dialogService = dialogService;
			AddProductCommand = new RelayCommand(_ => AddProduct());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			DeleteProductCommand = new RelayCommand<ProductModel>(DeleteProduct);
			RefreshCommand = new RelayCommand(async _ => await LoadProductsAsync());
		}
		public async Task Initialize()
		{
			await LoadCategoriesAsync();
		}
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

			if (SelectedCategory == null)
				return;

			var data = await _productService.GetProductByCategoryIdAsync(SelectedCategory.Id);
			foreach (var p in data)
			{
				Products.Add(_mapper.Map<ProductModel>(p));
			}
		}

		private void AddProduct()
		{
			if (SelectedCategory == null)
				return;

			int nextOrder = Products.Any()
				? Products.Max(p => p.DisplayOrder) + 1
				: 1;

			Products.Add(new ProductModel
				{
					CategoryId = SelectedCategory.Id,					
					Name = "New Product",
					IsActive = true,
					DisplayOrder = nextOrder
				}
				);
		}

		private async Task SaveAsync()
		{
			if (SelectedCategory == null)
				return;
			var products = _mapper.Map<List<ProductDto>>(Products);
			var response = await _productService.SaveBulkProductsAsync(products);
			if (response.MetaData.ResultCode == ResultCodes.Success)
				_notificationService.Success(UIConstants.SavedSuccessfully);
			else if (response.MetaData.ResultCode == ResultCodes.DuplicateNotAllowed)
				_notificationService.Warning(response.MetaData.ResultMessage);
			else
				_notificationService.Error(UIConstants.Error);
			await LoadProductsAsync();
		}
		private async void DeleteProduct(ProductModel? product)
		{
			if (product == null)
				return;

			var result = await _dialogService.ShowConfirmAsync("Confirm Delete", $"Are you sure you want to delete product \"{product.Name}\"?");

			if (!result)
				return;

			if (product.Id != 0)
			{
				var response = await _productService.DeleteProductAsync(product.Id);
				if (response.MetaData.ResultCode == ResultCodes.Success)
				{
					Products.Remove(product);
					_notificationService.Success(UIConstants.DeletedSuccessfully);
				}
				else
					_notificationService.Error(UIConstants.Error);
			}
		}
	}
}
