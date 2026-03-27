namespace CommBackend.Models.Data
{
    public class TeamsCall
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<User> Users { get; set; }
    }
}
