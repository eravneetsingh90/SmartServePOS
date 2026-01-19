using AutoMapper;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public class CatalogService : ICatalogService
	{
		#region fields
		private readonly IMapper _mapper;
		private readonly IMasterDataService _dataService;
		private readonly IProductService _productService;
		private readonly ITableStatusStore _tableStatusStore;
		private List<CategoryDto> _categories = new();
		private List<ProductDto> _products = new();
		private List<ProductVariantDto> _variants = new();
		private List<CatalogSearchItem> _searchIndex = new();
		private List<TableStatus> _tableStatus = new();
		private List<Brand> _brands = new();
		private bool _loaded;
		#endregion

		public CatalogService(
			IMapper mapper,
			IProductService productService,
			ITableStatusStore tableStatusStore, 
			IMasterDataService dataService)
		{
			_mapper = mapper;
			_productService = productService;
			_tableStatusStore = tableStatusStore;
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
			var tableStatus = (await _tableStatusStore.GetAllAsync()).ToList();
			_tableStatus = _mapper.Map<List<TableStatus>>(tableStatus);

			var brands = (await _productService.GetBrandsAsync())
				.Where(v => v.IsActive == true)
				.ToList();
			_brands = _mapper.Map<List<Brand>>(brands);
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

		public TableStatus GetTableStatusByCode(string statusCode)
			=> _tableStatus
				.Where(v => v.StatusCode.Equals(statusCode)).FirstOrDefault();

		public IReadOnlyList<Brand> GetBrands()
			=> _brands;
	}

}
