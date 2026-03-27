using CommBackend.Models.Data;

namespace CommBackend.Models.Presentation.Teams
{
    public class TeamsRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string TeamsMeetingUrl { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TeamsCall AsTeamsCall()
        {
            return new TeamsCall()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description,
                TeamsMeetingUrl = TeamsMeetingUrl,
                StartTime = StartTime,
                EndTime = EndTime
            };
        }
    }
}
