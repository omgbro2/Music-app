namespace WebApplication2.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}