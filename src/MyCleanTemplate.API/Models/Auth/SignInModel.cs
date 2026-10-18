namespace MyCleanTemplate.API.Models.Auth
{
    public class SignInModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        // Authenticator code; only needed when the user has MFA enabled
        public string? Token { get; set; }
    }
}
