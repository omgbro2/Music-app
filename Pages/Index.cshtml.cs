using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using WebApplication2.Models;


namespace WebApplication2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        public string Username { get; set; }

        public List<Playlist> Playlists { get; set; }

        public IndexModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            User? user = _userManager.GetUserAsync(User).Result;

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Username = user.UserName;


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
