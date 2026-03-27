using Livekit.Server.Sdk.Dotnet;
using Microsoft.Extensions.Configuration;

namespace CommBackend.Endpoints
{
    public static class CallEndpoints
    {
        public static void MapCallEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/rooms", async (RoomServiceClient client) =>
            {
                var response = await client.ListRooms(new ListRoomsRequest());
                return Results.Ok(response.Rooms);
            });

            // 2. Create a new room
            app.MapPost("/rooms/create", async (RoomServiceClient client, string roomName) =>
            {
                var request = new CreateRoomRequest
                {
                    Name = roomName,
                    EmptyTimeout = 600, // 10 minutes
                    MaxParticipants = 20
                };

                var room = await client.CreateRoom(request);
                return Results.Ok(room);
            });

            // 3. Generate a Join Token (Crucial for users to connect)
            app.MapPost("/rooms/join", (string roomName, string identity, IConfiguration config) =>
            {
                var apiKey = config["LiveKit:ApiKey"];
                var apiSecret = config["LiveKit:ApiSecret"];

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    return Results.Problem("LiveKit credentials are not configured.");
                }


                var token = new AccessToken(apiKey, apiSecret)
                    .WithIdentity(identity)
                    .WithGrants(new VideoGrants { RoomJoin = true, Room = roomName });

                return Results.Ok(new { token = token.ToJwt() });
            });
        }
    }
}
