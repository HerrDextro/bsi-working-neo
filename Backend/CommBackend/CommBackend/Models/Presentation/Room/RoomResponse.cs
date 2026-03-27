using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Auth;
using Livekit.Server.Sdk.Dotnet;
using System.Diagnostics.CodeAnalysis;

namespace CommBackend.Models.Presentation.Room
{
    public class RoomResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public int MemberCount { get; set; }
        public uint MaxParticipants { get; set; }

        public List<DisplayUser> Members { get; set; } = new List<DisplayUser>();

        [SetsRequiredMembers]
        public RoomResponse(Livekit.Server.Sdk.Dotnet.Room roomCall, RoomCall roomDb)
        {
            Id = roomDb.Id;
            Name = roomDb.Name;

            if (roomDb.MemberCount != roomCall.NumParticipants) { throw new Exception("Missmatch between call and db data"); }
            MemberCount = roomDb.MemberCount;

            MaxParticipants = roomCall.MaxParticipants;
            Members = new();

            foreach (var member in roomDb.Members)
            {
                Members.Add(new DisplayUser(member));
            }
        }

        [SetsRequiredMembers]
        public RoomResponse(RoomCall room, uint maxParticipants)
        {
            Id = room.Id;
            Name = room.Name;

            MemberCount = room.MemberCount;
            MaxParticipants = maxParticipants;
        }
    }
}
