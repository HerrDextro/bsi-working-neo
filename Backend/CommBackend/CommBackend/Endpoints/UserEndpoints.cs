using CommBackend.Models.Context;
using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Auth;
using CommBackend.Services.Hashing;
using CommBackend.Services.TokenGenerator;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CommBackend.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", async (UserRequest request, CommContext db, IPasswordHash hasher) =>
            {
                User user = request.AsDbUser(hasher);

                db.Users.Add(user);
                await db.SaveChangesAsync();
   
                return Results.Ok(new DisplayUser(user));
            });

            app.MapPost("/auth/login", async Task<Results<Ok<UserResponse>, BadRequest<string>>> (
                UserLoginRequest request,
                CommContext db,
                IPasswordHash hasher,
                ITokenGenerator tokenService,
                HttpResponse response) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Name == request.Username);

                if (user is null || !await hasher.VerifyPasssword(request.Password, user.Hash))
                {
                    return TypedResults.BadRequest("Invalid username or password.");
                }

                var accessToken = await tokenService.GenerateAccessToken(user);
                var refreshToken = await tokenService.GenerateRefreshToken();

                await tokenService.SetRefreshToken(refreshToken, response);

                user.RefreshToken = refreshToken.Token;
                user.DateCreated = refreshToken.Created;
                user.TokenExpires = refreshToken.Expires;

                await db.SaveChangesAsync();

                var userResponse = new UserResponse
                {
                    Token = accessToken,
                    Role = user.Role,
                    Uid = user.Id
                };

                return TypedResults.Ok(userResponse);
            }).WithName("Login");

            app.MapPost("/auth/refresh", async Task<Results<Ok<UserResponse>, UnauthorizedHttpResult>> (
                HttpResponse response,
                ITokenGenerator tokenService,
                CommContext db,
                HttpRequest request
                ) =>
            {
                request.Cookies.TryGetValue("MyCookieName", out var refreshToken); //not clean but alr // :<

                var existingUser = await db.Users!
                    .FirstOrDefaultAsync(user => user.RefreshToken == refreshToken);

                if (existingUser == null)
                {
                    return TypedResults.Unauthorized();
                }

                else if (existingUser.TokenExpires < DateTime.UtcNow)
                {
                    return TypedResults.Unauthorized();
                }
                else
                {
                    string acessToken = await tokenService.GenerateAccessToken(existingUser);
                    var newRefreshToken = await tokenService.GenerateRefreshToken();

                    existingUser.RefreshToken = newRefreshToken.Token;
                    existingUser.TokenExpires = newRefreshToken.Expires;
                    existingUser.DateCreated = newRefreshToken.Created;

                    await db.SaveChangesAsync();

                    await tokenService.SetRefreshToken(newRefreshToken, response);

                    UserResponse userResponse = new UserResponse()
                    {
                        Token = acessToken,
                        Role = existingUser.Role,
                        Uid = existingUser.Id
                    };

                    return TypedResults.Ok(userResponse);
                }
            });

            app.MapGet("/auth", async Task<Results<Ok<List<DisplayUser>>, BadRequest<string>>> (CommContext db) =>
            {
                var users = await db.Users
                    .Include(u => u.TeamsCall)
                    .Include(u => u.RoomCall)
                    .ToListAsync();

                var displayUsers = users.Select(user => new DisplayUser(user)).ToList(); //use linq!!

                return TypedResults.Ok(displayUsers);
            });

            app.MapGet("/auth/{uuid}", async Task<Results<Ok<DisplayUser>, NotFound<string>>> (CommContext db, string uuid) =>
            {
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == uuid);
                if (user == null) { return TypedResults.NotFound("User not found"); }

                return TypedResults.Ok(new DisplayUser(user));
            }).RequireAuthorization();
        }
    }
}
