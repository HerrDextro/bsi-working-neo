using CommBackend.Models.Data;
using CommBackend.Services.Hashing;

namespace CommBackend.Models.Presentation.Auth
{
    public class UserRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }


        public User AsDbUser(IPasswordHash hasher)
        {
            string hash = hasher.HashPassword(Password);
            
            return new User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description,
                Email = Email,
                Hash = hash
            };
        }
    }
}
