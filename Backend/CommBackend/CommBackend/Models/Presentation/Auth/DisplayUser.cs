using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Room;
using System.Diagnostics.CodeAnalysis;

namespace CommBackend.Models.Presentation.Auth
{
    public class DisplayUser
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Email { get; set; }
        public int Role { get; set; }

        public string? RoomCallId { get; set; }
        public string? TeamsCallId { get; set; }

        [SetsRequiredMembers]
        public DisplayUser(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Description = user.Description;
            Email = user.Email;
            Role = user.Role;

            RoomCallId = user.RoomId;
            TeamsCallId = user.TeamsId;
        }
    }
}
