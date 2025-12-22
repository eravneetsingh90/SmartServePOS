using SmartServe.Domain.Services;
using SmartServePOS.Command;
using SmartServePOS.Helper;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class BillingViewModel : BaseViewModel
	{
		private string _searchText;
		private bool _isSearchActive;
		private readonly IPrintService _printService;
		private int _currentOrderId = 1; // hardcoded for now
		private readonly ICatalogService _catalogService;
		public ICommand IncreaseQtyCommand { get; }
		public ICommand DecreaseQtyCommand { get; }
		public ICommand RemoveItemCommand { get; }
		public ObservableCollection<CategoryDto> Categories { get; }
		public ObservableCollection<ProductModelDto> Products { get; }
		public ObservableCollection<ProductVariantModelDto> Variants { get; }
		public ObservableCollection<BillItemModelDto> BillItems { get; }

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

		private ProductModelDto _selectedProduct;
		public ProductModelDto SelectedProduct
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

		public ICommand PrintCommand { get; }
		public ICommand AddVariantCommand { get; }

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

		// =============================
		// CONSTRUCTOR
		// =============================
		public BillingViewModel(ICatalogService catalogService, IPrintService printService)
		{
			_catalogService = catalogService;
			_printService = printService;
			
			IncreaseQtyCommand = new RelayCommand<BillItemModelDto>(IncreaseQty);
			DecreaseQtyCommand = new RelayCommand<BillItemModelDto>(DecreaseQty);
			RemoveItemCommand = new RelayCommand<BillItemModelDto>(RemoveItem);

			Categories = new ObservableCollection<CategoryDto>();
			Products = new ObservableCollection<ProductModelDto>();
			Variants = new ObservableCollection<ProductVariantModelDto>();
			BillItems = new ObservableCollection<BillItemModelDto>();

			AddVariantCommand = new RelayCommand<ProductVariantModelDto>(AddVariantToBill);
			PrintCommand = new RelayCommand<BillPrintModel>(PrintBill);
			LoadCategories();
		}

		private void LoadCategories()
		{
			Categories.Clear();

			foreach (var category in _catalogService.GetCategories())
			{
				Categories.Add(new CategoryDto
				{
					CategoryId = category.CategoryId,
					Name = category.Name
				});
			}

			SelectedCategory = Categories.FirstOrDefault();
		}

		private void LoadProducts()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory == null)
				return;

			var products = _catalogService.GetProductsByCategory(SelectedCategory.CategoryId);

			foreach (var product in products)
			{
				Products.Add(new ProductModelDto
				{
					CategoryId = product.CategoryId ?? 0,
					ProductId = product.ProductId,
					Name = product.Name
				});
			}

			SelectedProduct = Products.FirstOrDefault();
		}

		private void LoadVariants()
		{
			Variants.Clear();

			if (SelectedProduct == null)
				return;

			var variants = _catalogService.GetVariantsByProduct(SelectedProduct.ProductId);

			foreach (var variant in variants)
			{
				Variants.Add(new ProductVariantModelDto
				{
					ProductId = variant.ProductId,
					VariantId = variant.ProductVariantId,
					Price = variant.Price,
					VariantName = variant.Name
				});
			}
		}

		private void AddVariantToBill(ProductVariantModelDto variant)
		{
			var existing = BillItems.FirstOrDefault(x => x.VariantId == variant.VariantId);

			if (existing != null)
			{
				existing.Quantity++;
			}
			else
			{
				BillItems.Add(new BillItemModelDto
				{
					VariantId = variant.VariantId,
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
				Variants.Add(new ProductVariantModelDto
				{
					VariantId = item.VariantId,
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
			_printService.PrintBill(bill, showPreview: true);

			// Phase 2 (later):
			// _printService.PrintBill(bill, showPreview: false);
		}


	}
}


