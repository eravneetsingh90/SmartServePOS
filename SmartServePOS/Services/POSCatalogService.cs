using AutoMapper;
using SmartServe.Domain.Interfaces;
using SmartServe.Domain.Models;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public class POSCatalogService : IPOSCatalogService
	{
		#region fields
		private readonly IMasterDataService _dataService;
		private List<CategoryDto> _categories = new();
		private List<ProductDto> _products = new();
		private List<ProductVariantDto> _variants = new();
		private List<CatalogSearchItem> _searchIndex = new();
		private List<TableStatusDto> _tableStatus = new();
		private bool _loaded;
		#endregion

		public POSCatalogService(
			IProductService productService,
			IMasterDataService dataService)
		{
			_dataService = dataService;
		}

		public async Task LoadAsync()
		{
			if (_loaded)
				return;

			// Categories
			_categories = await _dataService.GetCategoriesAsync();
			// Products
			_products = await _dataService.GetProductsAsync();

			// Variants
			_variants = await _dataService.GetProductVariantsAsync();

			// Table Statuses
			_tableStatus = await _dataService.GetTableStatusAsync();

			// 🔍 Build search index
			_searchIndex =
				(from v in _variants
				 join p in _products on v.ProductId equals p.Id
				 join c in _categories on p.CategoryId equals c.Id
				 select new CatalogSearchItem
				 {
					 CategoryId = c.Id,
					 ProductId = p.Id,
					 VariantId = v.Id,
					 CategoryName = c.Name,
					 ProductName = p.Name,
					 VariantName = v.VariantName,
					 Price = v.Price,
					 SearchText =
						 (c.Name + " " + p.Name + " " + v.VariantName).ToLower()
				 })
				.ToList();

			_loaded = true;
		}

		public IReadOnlyList<CategoryDto> GetCategories()
			=> _categories;

		public IReadOnlyList<ProductDto> GetProductsByCategory(int categoryId)
			=> _products
				.Where(p => p.CategoryId == categoryId)
				.OrderBy(p => p.DisplayOrder)
				.ToList();

		public IReadOnlyList<ProductVariantDto> GetVariantsByProduct(int productId)
			=> _variants
				.Where(v => v.ProductId == productId)
				.OrderBy(v => v.DisplayOrder)
				.ToList();

		public IReadOnlyList<CatalogSearchItem> Search(string term, int maxResults = 30)
		{
			if (string.IsNullOrWhiteSpace(term))
				return Array.Empty<CatalogSearchItem>();

			term = term.Trim().ToLower();

			return _searchIndex
				.Where(x => x.SearchText.Contains(term))
				.Take(maxResults)
				.ToList();
		}

		public async Task Refresh()
		{
			_loaded = false;
			await LoadAsync();
		}

		public TableStatusDto GetTableStatusByCode(string statusCode)
			=> _tableStatus
				.Where(v => v.StatusCode.Equals(statusCode)).FirstOrDefault();

		
	}

}
