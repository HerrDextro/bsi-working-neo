namespace CommBackend.Services.Hashing
{
    public interface IPasswordHash
    {
        public string HashPassword(string password);
        public Task<bool> VerifyPasssword(string password, string passwordHash);
    }

}
