using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;

namespace WebApplication2.Pages.Playlists
{
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
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(_userId);
        }

        // CREATE PLAYLIST
        public async Task<IActionResult> OnPostAsync([FromForm] string playlistName)
        {
            if (!string.IsNullOrWhiteSpace(playlistName))
            {
                await _playlistRepository.CreatePlaylistAsync(_userId, playlistName);
            }

            // Reload playlists after creation
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(_userId);

            return Page();
        }

        // DELETE PLAYLIST
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _playlistRepository.DeletePlaylistAsync(id);

            // Reload playlists after deletion
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(_userId);

            return Page();
        }
    }
}