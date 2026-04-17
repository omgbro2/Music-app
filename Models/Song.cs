
namespace WebApplication2.Models
{
    public class Song
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public int Duration { get; set; }
        public DateTime DateAdded { get; set; }
        public string ContentType { get; set; }
    }
}