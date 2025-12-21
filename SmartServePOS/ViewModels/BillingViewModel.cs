using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class BillingViewModel : BaseViewModel
	{
		// =============================
		// STATE
		// =============================
		private int _currentOrderId = 1; // hardcoded for now

		public ICommand IncreaseQtyCommand { get; }
		public ICommand DecreaseQtyCommand { get; }
		public ICommand RemoveItemCommand { get; }

		// =============================
		// COLLECTIONS (BOUND TO UI)
		// =============================
		public ObservableCollection<CategoryModelDto> Categories { get; }
		public ObservableCollection<ProductModelDto> Products { get; }
		public ObservableCollection<ProductVariantModelDto> Variants { get; }
		public ObservableCollection<BillItemModelDto> BillItems { get; }

		// =============================
		// SELECTED CATEGORY
		// =============================
		private CategoryModelDto _selectedCategory;
		public CategoryModelDto SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				_selectedCategory = value;
				OnPropertyChanged(nameof(SelectedCategory));
				LoadProducts(); // hardcoded
			}
		}

		// =============================
		// SELECTED PRODUCT
		// =============================
		private ProductModelDto _selectedProduct;
		public ProductModelDto SelectedProduct
		{
			get => _selectedProduct;
			set
			{
				_selectedProduct = value;
				OnPropertyChanged(nameof(SelectedProduct));
				LoadVariants(); // hardcoded
			}
		}

		// =============================
		// TOTAL
		// =============================
		public decimal GrandTotal => BillItems.Sum(x => x.TotalPrice);

		// =============================
		// COMMANDS
		// =============================
		public ICommand AddVariantCommand { get; }

		// =============================
		// CONSTRUCTOR
		// =============================
		public BillingViewModel()
		{
			IncreaseQtyCommand = new RelayCommand<BillItemModelDto>(IncreaseQty);
			DecreaseQtyCommand = new RelayCommand<BillItemModelDto>(DecreaseQty);
			RemoveItemCommand = new RelayCommand<BillItemModelDto>(RemoveItem);

			Categories = new ObservableCollection<CategoryModelDto>();
			Products = new ObservableCollection<ProductModelDto>();
			Variants = new ObservableCollection<ProductVariantModelDto>();
			BillItems = new ObservableCollection<BillItemModelDto>();

			AddVariantCommand = new RelayCommand<ProductVariantModelDto>(AddVariantToBill);

			LoadCategories();
		}

		// =============================
		// LOAD CATEGORIES (HARDCODED)
		// =============================
		private void LoadCategories()
		{
			Categories.Clear();

			Categories.Add(new CategoryModelDto { CategoryId = 1, Name = "Ice Cream Scoops" });
			Categories.Add(new CategoryModelDto { CategoryId = 2, Name = "Burger" });

			SelectedCategory = Categories.First();
		}

		// =============================
		// LOAD PRODUCTS (HARDCODED)
		// =============================
		private void LoadProducts()
		{
			Products.Clear();
			Variants.Clear();

			if (SelectedCategory.Name == "Ice Cream Scoops")
			{
				Products.Add(new ProductModelDto { ProductId = 1, Name = "Vanilla" });
				Products.Add(new ProductModelDto { ProductId = 2, Name = "Chocolate" });
			}
			else if (SelectedCategory.Name == "Burger")
			{
				Products.Add(new ProductModelDto { ProductId = 10, Name = "Veg Burger" });
				Products.Add(new ProductModelDto { ProductId = 11, Name = "Non-Veg Burger" });
			}

			SelectedProduct = Products.FirstOrDefault();
		}

		// =============================
		// LOAD VARIANTS (HARDCODED)
		// =============================
		private void LoadVariants()
		{
			Variants.Clear();
			if (SelectedProduct != null)
			{
				if (SelectedProduct.Name == "Vanilla")
				{
					Variants.Add(new ProductVariantModelDto
					{
						VariantId = 101,
						ProductId = 1,
						VariantName = "Single Scoop",
						Price = 40,
						TracksStock = false
					});

					Variants.Add(new ProductVariantModelDto
					{
						VariantId = 102,
						ProductId = 1,
						VariantName = "Double Scoop",
						Price = 70,
						TracksStock = false
					});
				}
				else if (SelectedProduct.Name == "Veg Burger")
				{
					Variants.Add(new ProductVariantModelDto
					{
						VariantId = 201,
						ProductId = 10,
						VariantName = "Aloo Tikki Burger",
						Price = 50,
						TracksStock = false
					});
				}
			}
		}

		// =============================
		// ADD VARIANT TO BILL
		// =============================
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

	}
}


