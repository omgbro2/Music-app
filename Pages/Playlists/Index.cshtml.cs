using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace WebApplication2.Pages.Playlists
{
    public class IndexModel : PageModel
    {
        public List<string> Playlists { get; set; }

        public string Username { get; set; }

        public void OnGet()
        {
            Username = User.Identity?.Name ?? "Guest";

            Playlists = new List<string>
            {
                "Playlist 1",
                "Playlist 2",
                "Playlist 3"
            };
        }
    }
}
