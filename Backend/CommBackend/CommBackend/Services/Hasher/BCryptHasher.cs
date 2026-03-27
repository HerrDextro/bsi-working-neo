using BCrypt.Net;

namespace CommBackend.Services.Hashing
{
    public class BCryptHasher : IPasswordHash
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 13);
        }

        async public Task<bool> VerifyPasssword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
    }
}
