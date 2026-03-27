namespace CommBackend.Models.Data
{
    public class RoomCall
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public int MemberCount { get { return Members.Count; } }

        public List<User> Members { get; set; } = new List<User>();
    }
}
