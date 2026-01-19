using SmartServe.Domain.Models;
using SmartServe.EFCore.Models;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public interface IPOSCatalogService
	{
		Task LoadAsync();
		IReadOnlyList<CategoryDto> GetCategories();
		IReadOnlyList<ProductDto> GetProductsByCategory(int categoryId);
		IReadOnlyList<ProductVariantDto> GetVariantsByProduct(int productId);
		IReadOnlyList<CatalogSearchItem> Search(string term, int maxResults = 30);
		TableStatusDto GetTableStatusByCode(string statusCode);
		Task Refresh();
	}

}
