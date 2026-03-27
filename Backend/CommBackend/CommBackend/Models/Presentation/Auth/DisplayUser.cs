using CommBackend.Models.Data;
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

        public RoomCall? RoomCall { get; set; }
        public TeamsCall? TeamsCall { get; set; }

        [SetsRequiredMembers]
        public DisplayUser(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Description = user.Description;
            Email = user.Email;
            Role = user.Role;
            RoomCall = user.RoomCall;
            TeamsCall = user.TeamsCall;
        }
    }
}
