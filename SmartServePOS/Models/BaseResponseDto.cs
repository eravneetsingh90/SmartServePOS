namespace SmartServePOS.Models
{
    public class BaseResponseDto<T> : BaseResponseDto
    {
        public T Data { get; set; }
        
        public static BaseResponseDto<T> New<T>() where T : class, new()
        {
            return new BaseResponseDto<T>()
            {
                MetaData = new MetaDataDto()
            };
        }
        public static BaseResponseDto<T> New<T>(T data) where T : class, new()
        {
            return new BaseResponseDto<T>()
            {
                Data = data,
                MetaData = new MetaDataDto()
            };
        }
    }

    public class BaseResponseDto
    {
        public MetaDataDto MetaData { get; set; }
        public static BaseResponseDto New()
        {
            return new BaseResponseDto()
            {
                MetaData = new MetaDataDto()
            };
        }

    }
}
