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
                string metadata = $"{{\"title\": \"{roomName}\"}}";

                var request = new CreateRoomRequest
                {
                    Name = Guid.NewGuid().ToString(), // uses a guid and not name (because name is the pk of the calls)
                    EmptyTimeout = 3600, // 60 minutes
                    MaxParticipants = 20,
                    Metadata = metadata
                };

                var room = await client.CreateRoom(request);
                return Results.Ok(room);
            });

            // 3. Generate a Join Token (Crucial for users to connect)
            app.MapPost("/rooms/join", (string roomName, IConfiguration config) =>
            {
                var apiKey = config["LiveKit:ApiKey"];
                var apiSecret = config["LiveKit:ApiSecret"];
                string identity = "me"; //TODO replace with usr.uuid

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    return Results.Problem("LiveKit credentials are not configured.");
                }


                var token = new AccessToken(apiKey, apiSecret)
                    .WithIdentity(identity)
                    .WithGrants(new VideoGrants { RoomJoin = true, Room = roomName });

                return Results.Ok(new { token = token.ToJwt() });
            });

            app.MapDelete("/rooms/{roomName}", async (RoomServiceClient client, string roomName) =>
            {
                try
                {
                    await client.DeleteRoom(new DeleteRoomRequest
                    {
                        Room = roomName
                    });

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.NotFound(new { message = $"Could not delete room: {roomName}", error = ex.Message });
                }
            });

            app.MapPatch("/rooms/{roomName}/title", async (RoomServiceClient client, string roomName, string newTitle) =>
            {
                try
                {
                    string metadataJson = $"{{\"title\": \"{newTitle}\"}}"; // wrapping metadata for frontend

                    var updatedRoom = await client.UpdateRoomMetadata(new UpdateRoomMetadataRequest
                    {
                        Room = roomName,
                        Metadata = metadataJson
                    });

                    return Results.Ok(new { message = "Room title updated", room = updatedRoom });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Failed to delete room: {ex.Message}");
                }
            });
        }
    }
}
