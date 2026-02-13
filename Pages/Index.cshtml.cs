using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace WebApplication2.Pages
{
    public class IndexModel : PageModel
    {
        public string Username { get; set; }

        public List<Playlist> Playlists { get; set; }

        public IActionResult OnGet()
        {
            Username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(Username))
            {
                return RedirectToPage("/Account/Login");
            }

            Playlists = new List<Playlist>
            {
                new Playlist { Id = 1, Name = "Playlist 1" },
                new Playlist { Id = 2, Name = "Playlist 2" }
            };

            return Page();
        }

    }

    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
