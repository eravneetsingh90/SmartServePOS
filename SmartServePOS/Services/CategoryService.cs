using SmartServePOS.Constant;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IApiClientService _apiClient;

        public CategoryService(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponseDto<List<CategoryDto>>> GetAllAsync()
        {
            var response = await _apiClient.GetAsync<BaseResponseDto<List<CategoryDto>>>(UrlConstants.Categories);
            response.Data = response.Data.OrderBy(e => e.DisplayOrder).ToList();
            return response;
        }

        public async Task<BaseResponseDto<List<CategoryDto>>> GetActiveAsync()
        {
            var response = await GetAllAsync();
            response.Data = response.Data.Where(c => c.IsActive).ToList();
            return response;
        }

        public async Task<BaseResponseDto<CategoryDto>> GetByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync<BaseResponseDto<CategoryDto>>($"{UrlConstants.Categories}/{id}");

            return response;
        }

        public async Task<BaseResponseDto<CategoryDto>> CreateAsync(CreateCategoryRequestDto request)
        {
            var response = await _apiClient.PostAsync<BaseResponseDto<CategoryDto>>(UrlConstants.Categories, request);

            return response;
        }

        public async Task<BaseResponseDto<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequestDto request)
        {
            var response = await _apiClient.PutAsync<BaseResponseDto<CategoryDto>>($"{UrlConstants.Categories}/{id}", request);

            return response;
        }

        public async Task<BaseResponseDto> DeleteAsync(int id)
        {
            var response = await _apiClient.DeleteAsync<BaseResponseDto>($"{UrlConstants.Categories}/{id}");
            return response;
        }

        public async Task<BaseResponseDto> BulkUpdateAsync(List<CategoryDto> request)
        {
            var response = await _apiClient.PostAsync<BaseResponseDto<List<CategoryDto>>>($"{UrlConstants.Categories}/bulk/update", request);

            return response;
        }
    }
}
