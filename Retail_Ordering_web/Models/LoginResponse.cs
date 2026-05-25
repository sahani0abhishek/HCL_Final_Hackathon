namespace Retail_Ordering_web.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }
}
