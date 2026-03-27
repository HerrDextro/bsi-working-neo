namespace CommBackend.Models.Presentation.Auth
{
    public class UserLoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
