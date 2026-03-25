using System.Security.Claims;

namespace WebApplication2.Models

{
    public class Playlist
    {
        public int Id { get; set; }            // Playlist ID
        public string Name { get; set; }       // Playlist Name
        public string DateCreated { get; set; } // Optional
        public string UserId { get; set; }      // Optional
    }
}