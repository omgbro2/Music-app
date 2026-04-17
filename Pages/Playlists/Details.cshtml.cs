using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Pages.Playlists
{
    [Authorize]
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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            PlaylistId = id;

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id, userId);
            if (playlist == null)
            {
                return NotFound();
            }

            PlaylistName = playlist.Name;

            Songs = await _playlistRepository.GetSongsByPlaylistAsync(id, userId);

            return Page();
        }

        public async Task<IActionResult> OnPostAddSongAsync(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Artist) || Duration <= 0)
            {
                Songs = await _playlistRepository.GetSongsByPlaylistAsync(id, userId);
                var playlist = await _playlistRepository.GetPlaylistByIdAsync(id, userId);
                PlaylistName = playlist?.Name ?? "Playlist";
                PlaylistId = id;
                return Page();
            }
            throw new NotImplementedException(); // TO DO
            //await _playlistRepository.AddSongAsync(id, Title, Artist, Duration, userId);

            return RedirectToPage(new { id });
        }
    }
}