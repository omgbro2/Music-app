namespace WebApplication2.Models
{
    public class Song
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public DateTime DateAdded { get; set; }
    }
}