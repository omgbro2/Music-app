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

        public List<Playlist> Playlists { get; set; } = new List<Playlist>();

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


            

            return Page();
        }

    }

    
}
