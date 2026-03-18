using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.Pages.Playlists
{
    public class DetailsModel : PageModel
    {
        private readonly PlaylistRepository _playlistRepository;

        public DetailsModel(PlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public string PlaylistName { get; set; } = "Playlist not found";
        public List<Song> Songs { get; set; } = new();

        public int PlaylistId { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Artist { get; set; }

        [BindProperty]
        public int Duration { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PlaylistId = id;

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id);
            if (playlist == null)
            {
                // Playlist not found - return 404 or show message
                return NotFound();
            }

            PlaylistName = playlist.Name;

            Songs = await _playlistRepository.GetSongsByPlaylistAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAddSongAsync(int id)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Artist) || Duration <= 0)
            {
                // Invalid input - reload page with error
                Songs = await _playlistRepository.GetSongsByPlaylistAsync(id);
                var playlist = await _playlistRepository.GetPlaylistByIdAsync(id);
                PlaylistName = playlist?.Name ?? "Playlist";
                PlaylistId = id;
                return Page();
            }

            await _playlistRepository.AddSongAsync(id, Title, Artist, Duration);

            return RedirectToPage(new { id });
        }
    }
}