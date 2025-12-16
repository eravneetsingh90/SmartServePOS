using SmartServePOS.Command;
using SmartServePOS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SmartServePOS.ViewModels
{
	public class BillingViewModel : BaseViewModel
	{
		// =============================
		// Collections
		// =============================

		public ObservableCollection<CategoryModelDto> Categories { get; }
		public ObservableCollection<ProductModelDto> AllProducts { get; }
		public ObservableCollection<ProductModelDto> Products { get; }
		public ObservableCollection<BillItemModelDto> BillItems { get; }

		// =============================
		// Selected Category
		// =============================

		private CategoryModelDto _selectedCategory;
		public CategoryModelDto SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				_selectedCategory = value;
				OnPropertyChanged(nameof(SelectedCategory));
				FilterProducts();
			}
		}

		// =============================
		// Total
		// =============================

		public decimal GrandTotal => BillItems.Sum(x => x.TotalPrice);

		// =============================
		// Commands
		// =============================

		public ICommand AddProductCommand { get; }

		// =============================
		// Constructor
		// =============================

		public BillingViewModel()
		{
			Categories = new ObservableCollection<CategoryModelDto>();
			AllProducts = new ObservableCollection<ProductModelDto>();
			Products = new ObservableCollection<ProductModelDto>();
			BillItems = new ObservableCollection<BillItemModelDto>();

			AddProductCommand = new RelayCommand<ProductModelDto>(AddProductToBill);

			LoadDummyData();
		}

		// =============================
		// Dummy Data (DB-ACCURATE)
		// =============================

		private void LoadDummyData()
		{
			// ---- categories ----
			Categories.Add(new CategoryModelDto { CategoryId = 1, Name = "Scoops" });
			Categories.Add(new CategoryModelDto { CategoryId = 2, Name = "Waffles" });
			Categories.Add(new CategoryModelDto { CategoryId = 3, Name = "Ice Cream Cake" });

			// ---- products ----
			AllProducts.Add(new ProductModelDto
			{
				ProductId = 101,
				Name = "Single Scoop Vanilla",
				CategoryId = 1,
				ServingTypeId = 1,   // SCOOP
				FlavorId = 1,
				BrandId = null,
				Price = 80
			});

			AllProducts.Add(new ProductModelDto
			{
				ProductId = 102,
				Name = "Double Scoop Chocolate",
				CategoryId = 1,
				ServingTypeId = 2,   // DOUBLE SCOOP
				FlavorId = 2,
				BrandId = null,
				Price = 150
			});

			AllProducts.Add(new ProductModelDto
			{
				ProductId = 201,
				Name = "Belgian Chocolate Waffle",
				CategoryId = 2,
				ServingTypeId = 3,   // WAFFLE
				BrandId = null,
				Price = 220
			});

			AllProducts.Add(new ProductModelDto
			{
				ProductId = 301,
				Name = "Black Forest Ice Cream Cake",
				CategoryId = 3,
				ServingTypeId = 4,   // CAKE
				BrandId = 1,
				Price = 750
			});

			SelectedCategory = Categories.First();
		}

		// =============================
		// Logic
		// =============================

		private void FilterProducts()
		{
			Products.Clear();

			if (SelectedCategory == null)
				return;

			foreach (var product in AllProducts
						 .Where(p => p.CategoryId == SelectedCategory.CategoryId))
			{
				Products.Add(product);
			}
		}

		private void AddProductToBill(ProductModelDto product)
		{
			var existing = BillItems.FirstOrDefault(x => x.ProductId == product.ProductId);

			if (existing != null)
			{
				existing.Quantity++;
			}
			else
			{
				BillItems.Add(new BillItemModelDto
				{
					ProductId = product.ProductId,
					ItemName = product.Name,
					Quantity = 1,
					PriceSnapshot = product.Price
				});
			}

			OnPropertyChanged(nameof(GrandTotal));
		}
	}

}
