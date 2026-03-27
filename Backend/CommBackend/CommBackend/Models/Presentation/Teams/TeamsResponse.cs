using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Auth;
using System.Diagnostics.CodeAnalysis;

namespace CommBackend.Models.Presentation.Teams
{
    public class TeamsResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string TeamsMeetingUrl { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<DisplayUser> Users { get; set; } = new List<DisplayUser>();

        [SetsRequiredMembers]
        public TeamsResponse(TeamsCall call)
        {
            Id = call.Id;
            Name = call.Name;
            Description = call.Description;
            TeamsMeetingUrl = call.TeamsMeetingUrl;
            StartTime = call.StartTime;
            EndTime = call.EndTime;

            Users = call.Users.Select(user => new DisplayUser(user)).ToList();
        }
    }
}
