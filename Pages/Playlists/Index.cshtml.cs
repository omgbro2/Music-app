using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication2.Models;

namespace WebApplication2.Pages.Playlists
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly PlaylistRepository _playlistRepository;

        public IndexModel(UserManager<User> userManager, PlaylistRepository playlistRepository)
        {
            _userManager = userManager;
            _playlistRepository = playlistRepository;
        }

        public string Username { get; set; }
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();

        [BindProperty]
        public string NewPlaylistName { get; set; }

        [BindProperty]
        public int EditPlaylistId { get; set; }

        [BindProperty]
        public string EditPlaylistName { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                RedirectToPage("/Account/Login");
                return;
            }

            Username = user.UserName;
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(user.Id);
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (!string.IsNullOrWhiteSpace(NewPlaylistName))
            {
                await _playlistRepository.CreatePlaylistAsync(user.Id, NewPlaylistName);
            }

            return RedirectToPage();
        }
    }
}