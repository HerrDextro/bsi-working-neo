using Livekit.Server.Sdk.Dotnet;

namespace CommBackend.Models.Data
{
    public class User
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public int Role {  get; set; }



        public string? RoomId { get; set; }

        public string? TeamsId { get; set; }
        public TeamsCall? TeamsCall { get; set; }


    }
}
