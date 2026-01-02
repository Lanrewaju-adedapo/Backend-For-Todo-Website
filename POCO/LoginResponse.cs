namespace TestProject.POCO
{
    public class LoginResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public dynamic? UserInfo { get; set; }
    }
}
