using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;

namespace WebApplication2.Pages.Playlists
{
    public class DetailsModel : PageModel
    {
        private readonly PlaylistRepository _playlistRepository;

        public DetailsModel(PlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public string PlaylistName { get; set; }
        public List<Song> Songs { get; set; } = new();

        public int PlaylistId { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Artist { get; set; }

        [BindProperty]
        public int Duration { get; set; }

        // --------------------
        // LOAD PAGE
        // --------------------
        public async Task OnGetAsync(int id)
        {
            PlaylistId = id;

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id);
            PlaylistName = playlist?.Name ?? "Playlist";

            Songs = await _playlistRepository.GetSongsByPlaylistAsync(id);
        }

        // --------------------
        // ADD SONG
        // --------------------
        public async Task<IActionResult> OnPostAddSongAsync(int id)
        {
            await _playlistRepository.AddSongAsync(
                id,
                Title,
                Artist,
                Duration
            );

            return RedirectToPage(new { id });
        }
    }
}