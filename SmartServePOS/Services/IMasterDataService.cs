using SmartServePOS.Models;

namespace SmartServePOS.Services
{
    public interface IMasterDataService
    {
		Task SyncAsync();
		Task<List<CategoryDto>> GetCategoriesAsync();
		Task<List<ProductDto>> GetProductsAsync();
		Task<List<ProductVariantDto>> GetProductVariantsAsync();
	}
}
