namespace SmartServePOS.Models
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }

        public string Username { get; set; } = default!;

        public string Role { get; set; } = default!;
    }
}
