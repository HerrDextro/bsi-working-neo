using CommBackend.Models.Context;
using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Room;
using CommBackend.Services.TokenGenerator;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.EntityFrameworkCore;

namespace CommBackend.Endpoints
{
    public static class CallEndpoints
    {
        public static void MapCallEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/rooms", async (RoomServiceClient client, CommContext db) =>
            {
                var response = await client.ListRooms(new ListRoomsRequest());
                var callRooms = response.Rooms;
                var dbRooms = await db.RoomCalls.Include(r => r.Members).ToListAsync();

                var displayResponse = dbRooms
                    .Join(
                        callRooms,
                        dbRoom => dbRoom.Id,
                        callRoom => callRoom.Name,
                        (dbRoom, callRoom) => new RoomResponse(callRoom, dbRoom)
                    )
                    .ToList();

                return Results.Ok(displayResponse);
            });

            app.MapPost("/rooms/create", async (RoomServiceClient client, CommContext db, string roomName) =>
            {
                string guid = Guid.NewGuid().ToString();
                uint maxParticipants = 20;

                var request = new CreateRoomRequest
                {
                    Name = guid,
                    EmptyTimeout = 3600,
                    MaxParticipants = maxParticipants,
                };

                var room = await client.CreateRoom(request);

                var dbRoom = new RoomCall()
                {
                    Id = guid,
                    Name = roomName,
                };

                db.RoomCalls.Add(dbRoom);
                await db.SaveChangesAsync();

                return Results.Ok(new RoomResponse(dbRoom, maxParticipants));
            });

            app.MapPost("/rooms/join", async (string roomId, IConfiguration config) =>
            {
                var apiKey = config["LiveKit:ApiKey"];
                var apiSecret = config["LiveKit:ApiSecret"];
                string identity = "abc"; //fetch from jwt later

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                    return Results.Problem("LiveKit credentials not configured.");

                var token = new AccessToken(apiKey, apiSecret)
                    .WithIdentity(identity)
                    .WithGrants(new VideoGrants { RoomJoin = true, Room = roomId });

                return Results.Ok(new { token = token.ToJwt() });
            });

            app.MapPost("/rooms/leave", async (RoomServiceClient client, CommContext db, string roomId, string identity) =>
            {
                try
                {
                    await client.RemoveParticipant(new RoomParticipantIdentity
                    {
                        Room = roomId,
                        Identity = identity
                    });

                    var room = await db.RoomCalls.Include(r => r.Members)
                                       .FirstOrDefaultAsync(r => r.Id == roomId);

                    if (room != null)
                    {
                        var member = room.Members.FirstOrDefault(m => m.Id == identity);
                        if (member != null) room.Members.Remove(member);

                        await db.SaveChangesAsync();
                    }

                    return Results.Ok(new { message = "Successfully left the room" });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = "Error leaving room", error = ex.Message });
                }
            });

            app.MapDelete("/rooms/{roomId}", async (RoomServiceClient client, string roomId, CommContext db) =>
            {
                try
                {
                    await client.DeleteRoom(new DeleteRoomRequest { Room = roomId });

                    var roomCall = await db.RoomCalls.FindAsync(roomId);
                    if (roomCall != null)
                    {
                        db.RoomCalls.Remove(roomCall);
                        await db.SaveChangesAsync();
                    }

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.NotFound(new { message = $"Could not delete room: {roomId}", error = ex.Message });
                }
            });

            app.MapPatch("/rooms/{roomId}/title", async (RoomServiceClient client, string roomId, string newTitle) =>
            {
                try
                {
                    string metadataJson = $"{{\"title\": \"{newTitle}\"}}";

                    var updatedRoom = await client.UpdateRoomMetadata(new UpdateRoomMetadataRequest
                    {
                        Room = roomId,
                        Metadata = metadataJson
                    });

                    return Results.Ok(new { message = "Room title updated", room = updatedRoom });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Failed to update room: {ex.Message}");
                }
            });
        }
    }
}