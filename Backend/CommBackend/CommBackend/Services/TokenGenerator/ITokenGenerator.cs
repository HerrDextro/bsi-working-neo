using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Auth;

namespace CommBackend.Services.TokenGenerator
{
    public interface ITokenGenerator
    {
        public Task<string> GenerateAccessToken(User user);
        public Task<RefreshToken> GenerateRefreshToken();
        public Task SetRefreshToken(RefreshToken newRefreshToken, HttpResponse Response);
        public Task<ClaimData> GetClaims(System.Security.Claims.ClaimsPrincipal user);
    }
}
