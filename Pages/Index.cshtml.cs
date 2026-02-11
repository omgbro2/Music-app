using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace WebApplication2.Pages
{
    public class IndexModel : PageModel
    {
        public string Username { get; set; }

        public List<Playlist> Playlists { get; set; }

        public void OnGet()
        {
            Username = "admin";

            Playlists = new List<Playlist>
            {
                new Playlist { Id = 1, Name = "Playlist 1" },
                new Playlist { Id = 2, Name = "Playlist 2" }
            };
        }
    }

    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
