namespace CommBackend.Models.Presentation.Auth
{
    public class UserResponse
    {
        public required string Token { get; set; }
        public required string Uid { get; set; }
        public int Role { get; set; }
    }
}
