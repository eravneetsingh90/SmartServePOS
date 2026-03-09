
using SmartServe.Common.Models;
using SmartServe.Domain.Models;
using SmartServePOS.Constant;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiClientService _apiClient;

        public AuthService(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponseDto<LoginResponseDto>> LoginAsync(string username, string pin)
        {
            var response = new BaseResponseDto<LoginResponseDto>();

            response = await _apiClient.PostAsync<BaseResponseDto<LoginResponseDto>>(
                UrlConstants.Login, new {
                    username = username,
                    pin = pin
                });

            if (!string.IsNullOrEmpty(response.Data.AccessToken))
            {
                _apiClient.SetToken(response.Data.AccessToken);
            }

            return response;
        }

        private BaseResponseDto<T> WithMappedError<T>(BaseResponseDto<T> response, string resultCode, string? resultMessage)
        {
            response.MetaData.ResultCode = resultCode;
            if (!string.IsNullOrWhiteSpace(resultMessage))
                response.MetaData.ResultMessage = resultMessage;
            return response;
        }
    }
}