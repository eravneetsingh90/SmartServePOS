using AutoMapper;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.EFCore.Models;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class BillingViewModel : BaseViewModel
	{
		#region commands
		private readonly IMapper _mapper;
		public ICommand IncreaseQtyCommand { get; }
		public ICommand DecreaseQtyCommand { get; }
		public ICommand RemoveItemCommand { get; }
		public ICommand ReloadMenuCommand { get; }
		public ICommand PrintCommand { get; }
		public ICommand AddVariantCommand { get; }
		public ICommand SaveCommand { get; }
		#endregion

		#region collections
		public ObservableCollection<CategoryDto> Categories { get; }
		public ObservableCollection<ProductModel> Products { get; }
		public ObservableCollection<ProductVariantDto> Variants { get; }
		public ObservableCollection<BillItemModelDto> BillItems { get; }
		#endregion

		#region services
		private readonly IPrintService _printService;
		private readonly ICatalogService _catalogService;
		private readonly IBillingService _billingService;
		private readonly INavigationService _navigationService;

		#endregion

		#region properties
		private bool _isEditable = true;
		private int _currentOrderId = 0;
		private int _currentTableId = 0;
		private string _searchText;
		private bool _isSearchActive;
		private CategoryDto _selectedCategory;
		public CategoryDto SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				_selectedCategory = value;
				OnPropertyChanged(nameof(SelectedCategory));
				LoadProducts();
			}
		}
		private ProductModel _selectedProduct;
		public ProductModel SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				_selectedProduct = value;
				OnPropertyChanged(nameof(SelectedProduct));
				LoadVariants();
			}
		}
		public decimal GrandTotal => BillItems.Sum(x => x.TotalPrice);
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

		public bool IsEditable
		{
			get => _isEditable;
			private set
			{
				_isEditable = value;
				OnPropertyChanged(nameof(IsEditable));
			}
		}
#endregion

		#region constructors
		public BillingViewModel(
			IMapper mapper,
			ICatalogService catalogService,
			IPrintService printService,
			IBillingService billingService,
			INavigationService navigationService)
		{
			_mapper = mapper;
			_catalogService = catalogService;
			_printService = printService;
			_billingService = billingService;
			_navigationService = navigationService;
			IncreaseQtyCommand = new RelayCommand<BillItemModelDto>(IncreaseQty);
			DecreaseQtyCommand = new RelayCommand<BillItemModelDto>(DecreaseQty);
			RemoveItemCommand = new RelayCommand<BillItemModelDto>(RemoveItem);
			ReloadMenuCommand = new RelayCommand(_ => RefreshAsync());
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
			AddVariantCommand = new RelayCommand<ProductVariantDto>(AddVariantToBill);
			PrintCommand = new RelayCommand<BillPrintModel>(PrintBill);

			Categories = new ObservableCollection<CategoryDto>();
			Products = new ObservableCollection<ProductModel>();
			Variants = new ObservableCollection<ProductVariantDto>();
			BillItems = new ObservableCollection<BillItemModelDto>();
			LoadCategories();
		}
		#endregion

		#region methods
		private void LoadCategories()
		{
			Categories.Clear();

			foreach (var category in _catalogService.GetCategories())
			{
				Categories.Add(new CategoryDto
				{
					Id = category.Id,
					Name = category.Name
				});
			}
			if (SelectedCategory == null)
				SelectedCategory = Categories.FirstOrDefault();
		}
		private void LoadProducts()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = _catalogService.GetProductsByCategory(SelectedCategory.Id);

			foreach (var product in products)
			{
				Products.Add(_mapper.Map<ProductModel>(product));
			}
			if (SelectedProduct == null)
				SelectedProduct = Products.FirstOrDefault();
		}
		private void LoadVariants()
		{
			Variants.Clear();

			if (SelectedProduct == null)
				return;

			var variants = _catalogService.GetVariantsByProduct(SelectedProduct.Id);

			foreach (var variant in variants)
			{
				Variants.Add(new ProductVariantDto
				{
					ProductId = variant.ProductId,
					Id = variant.Id,
					Price = variant.Price,
					VariantName = variant.VariantName
				});
			}
		}
		private void AddVariantToBill(ProductVariantDto variant)
		{
			var existing = BillItems.FirstOrDefault(x => x.VariantId == variant.Id);

			if (existing != null)
			{
				existing.Quantity++;
			}
			else
			{
				BillItems.Add(new BillItemModelDto
				{
					VariantId = variant.Id,
					ItemName = $"{SelectedProduct.Name} - {variant.VariantName}",
					Quantity = 1,
					PriceSnapshot = variant.Price
				});
			}

			OnPropertyChanged(nameof(GrandTotal));
		}
		private void IncreaseQty(BillItemModelDto item)
		{
			if (item == null) return;

			item.Quantity++;
			OnPropertyChanged(nameof(GrandTotal));
		}
		private void DecreaseQty(BillItemModelDto item)
		{
			if (item == null) return;

			if (item.Quantity > 1)
			{
				item.Quantity--;
			}
			else
			{
				BillItems.Remove(item);
			}

			OnPropertyChanged(nameof(GrandTotal));
		}
		private void RemoveItem(BillItemModelDto item)
		{
			if (item == null) return;

			BillItems.Remove(item);
			OnPropertyChanged(nameof(GrandTotal));
		}
		private void PerformSearch()
		{
			Variants.Clear();

			if (string.IsNullOrWhiteSpace(SearchText))
			{
				IsSearchActive = false;

				// restore normal flow
				if (SelectedProduct != null)
					LoadVariants();

				return;
			}

			IsSearchActive = true;

			var results = _catalogService.Search(SearchText);

			foreach (var item in results)
			{
				Variants.Add(new ProductVariantDto
				{
					Id = item.VariantId,
					ProductId = item.ProductId,
					VariantName = $"{item.ProductName} - {item.VariantName}",
					Price = item.Price
				});
			}
		}
		private BillPrintModel BuildBillPrintModel()
		{
			return new BillPrintModel
			{
				ShopName = "Scoop Ice Cream Cafe",
				Address = "Sco 8, Basement, Fortune City Center\nSec. 123, Mohali-140301",

				//BillNo = _currentOrder.OrderNumber,
				//TableName = _currentOrder.TableName ?? "N/A",
				//Cashier = _currentUser?.Name ?? "biller",

				PrintedAt = DateTime.Now,

				Items = BillItems.Select(x => new BillPrintItem
				{
					Name = x.ItemName,
					Quantity = x.Quantity,
					UnitPrice = x.PriceSnapshot
				}).ToList(),

				SubTotal = BillItems.Sum(x => x.Quantity * x.PriceSnapshot),
				//Discount = AppliedDiscountAmount,          // 0 if none
				//DiscountLabel = AppliedDiscountLabel,       // "10%" or ""
				GrandTotal = GrandTotal
			};
		}
		private void PrintBill(BillPrintModel bill)
		{
			if (!BillItems.Any())
				return;

			bill = BuildBillPrintModel();

			// Phase 1: preview first
			_printService.PrintBill(bill, showPreview: false);

			// Phase 2 (later):
			// _printService.PrintBill(bill, showPreview: false);
		}
		private async Task SaveAsync()
		{
			if (!BillItems.Any())
				return;
			var statusId = _catalogService.GetTableStatusByCode(TableStatusCodes.RUNNING).Id;

			if (_currentOrderId <= 0)
			{
				var request = new OrderDto
				{
					Id = _currentOrderId,
					TableId = _currentTableId,
					StatusId = statusId,
					OrderType = "DINE_IN",
					TotalAmount = GrandTotal
				};

				_currentOrderId = await _billingService.CreateOrderAsync(request);

				var orderItems = BillItems.Select(x => new OrderItemDto
				{
					OrderId = _currentOrderId,
					VariantId = x.VariantId,
					Quantity = x.Quantity,
					PriceSnapshot = x.PriceSnapshot
				}).ToList();

				await _billingService.CreateOrderItemsAsync(orderItems);
			}
			else 
			{
				var request = new OrderDto
				{
					Id = _currentOrderId,
					TableId = _currentTableId,
					StatusId = statusId,
					OrderType = "DINE_IN",
					TotalAmount = GrandTotal
				};

				await _billingService.UpdateOrderAsync(request);

				var orderItems = BillItems.Select(x => new OrderItemDto
				{
					OrderId = _currentOrderId,
					VariantId = x.VariantId,
					Quantity = x.Quantity,
					PriceSnapshot = x.PriceSnapshot
				}).ToList();

				await _billingService.UpdateOrderItemsAsync(_currentOrderId,orderItems);
			}
			// optional: clear bill after save
			BillItems.Clear();
			OnPropertyChanged(nameof(GrandTotal));
			_navigationService.NavigateToTableView();
		}
		public async Task LoadOrderAsync(int orderId, int tableId)
		{
			BillItems.Clear();
			_currentTableId = tableId;
			_currentOrderId = orderId;
			if (_currentOrderId <= 0)
			{
				return;
			}

			var order = await _billingService.GetOrderAsync(_currentOrderId);
			if (order == null)
				return;

			foreach (var item in order.OrderItems)
			{
				BillItems.Add(new BillItemModelDto
				{
					VariantId = item.VariantId ?? 0,
					ItemName = item.Variant.Product.Name + " - " + item.Variant?.VariantName,
					Quantity = item.Quantity,
					PriceSnapshot = item.PriceSnapshot
				});
			}
			OnPropertyChanged(nameof(GrandTotal));
		}
		public async void RefreshAsync()
		{
			await _catalogService.Refresh();
			LoadCategories();
		}
		#endregion
	}
}


