using SmartServePOS.Models;

namespace SmartServePOS.Services
{
    public interface IAuthService
    {
        Task<BaseResponseDto<LoginResponseDto>> LoginAsync(string username, string pin);
    }
}
