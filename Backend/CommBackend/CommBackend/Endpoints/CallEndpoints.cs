using CommBackend.Models.Context;
using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Auth;
using CommBackend.Models.Presentation.Room;
using CommBackend.Services.TokenGenerator;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            }).RequireAuthorization();

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
            }).RequireAuthorization();

            app.MapPost("/rooms/join", async (string roomId, IConfiguration config, ITokenGenerator tokenGenerator, ClaimsPrincipal user, CommContext db) =>
            {
                ClaimData claims = await tokenGenerator.GetClaims(user); //ported from controller api, fix!
                
                var apiKey = config["LiveKit:ApiKey"];
                var apiSecret = config["LiveKit:ApiSecret"];
                string identity = claims.Uuid;

                // add user to db
                RoomCall? call = db.RoomCalls.FirstOrDefault(c => c.Id == roomId);
                if (call == null) { return Results.NotFound(); }

                User? currentUser = db.Users.FirstOrDefault(u => u.Id == roomId);
                if (currentUser == null) { return Results.NotFound(); }

                call.Members.Add(currentUser);

                // return auth for user to join call
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                    return Results.Problem("LiveKit credentials not configured.");

                var token = new AccessToken(apiKey, apiSecret)
                    .WithIdentity(identity)
                    .WithGrants(new VideoGrants { RoomJoin = true, Room = roomId });

                return Results.Ok(new { token = token.ToJwt() });
            }).RequireAuthorization();

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
            }).RequireAuthorization();

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
            }).RequireAuthorization();

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
            }).RequireAuthorization();
        }
    }
}