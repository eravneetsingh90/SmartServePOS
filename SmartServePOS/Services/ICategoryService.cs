using SmartServePOS.Models;

namespace SmartServePOS.Services
{
    public interface ICategoryService
    {
        Task<BaseResponseDto<List<CategoryDto>>> GetAllAsync();
        Task<BaseResponseDto<CategoryDto>> GetByIdAsync(int id);
        Task<BaseResponseDto<CategoryDto>> CreateAsync(CreateCategoryRequestDto request);
        Task<BaseResponseDto<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequestDto request);
        Task<BaseResponseDto> DeleteAsync(int id);
        Task<BaseResponseDto> BulkUpdateAsync(List<CategoryDto> request);
    }
}
