using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.Pages.Playlists
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly PlaylistRepository _playlistRepository;

        public IndexModel(PlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public List<Playlist> Playlists { get; set; } = new();

        [BindProperty]
        public string PlaylistName { get; set; }

            // LOAD PLAYLISTS
        public async Task OnGetAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);
        }

        // CREATE PLAYLIST
        public async Task<IActionResult> OnPostAsync([FromForm] string playlistName)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!string.IsNullOrWhiteSpace(playlistName))
            {
                await _playlistRepository.CreatePlaylistAsync(userId, playlistName);
            }

            // Reload playlists after creation
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);

            return Page();
        }

        // DELETE PLAYLIST
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await _playlistRepository.DeletePlaylistAsync(id, userId);

            return RedirectToPage();
        }
    }
}