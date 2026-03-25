using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

        // --- PROPERTIES ---
        public List<Playlist> Playlists { get; set; } = new();

        // Selected playlist shown on the right
        public Playlist CurrentPlaylist { get; set; }

        [BindProperty]
        public string PlaylistName { get; set; }

        // Ðie lauki ir nepiecieðami dziesmu pievienoðanai (lai nebûtu kïûdu CS1061)
        [BindProperty]
        public int TargetPlaylistId { get; set; }

        [BindProperty]
        public string SongTitle { get; set; }

        [BindProperty]
        public string SongArtist { get; set; }

        [BindProperty]
        public int Minutes { get; set; }

        [BindProperty]
        public int Seconds { get; set; }

        // --- HANDLERS ---

        // IELÂDÇT PLAYLISTES
        // Accept optional id to select a playlist when user clicks on the left side
        public async Task OnGetAsync(int? id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);

            // Choose the requested playlist if present, otherwise the first one
            CurrentPlaylist = id.HasValue
                ? Playlists.FirstOrDefault(p => p.Id == id.Value)
                : Playlists.FirstOrDefault();

            // set TargetPlaylistId so forms bound to it default to the selected playlist
            TargetPlaylistId = CurrentPlaylist?.Id ?? 0;
        }

        // IZVEIDOT JAUNU PLAYLISTI
        public async Task<IActionResult> OnPostAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!string.IsNullOrWhiteSpace(PlaylistName))
            {
                await _playlistRepository.CreatePlaylistAsync(userId, PlaylistName);
            }

            return RedirectToPage();
        }

        // PIEVIENOT DZIESMU KONKRÇTAI PLAYLISTEI
        public async Task<IActionResult> OnPostAddSongAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!string.IsNullOrWhiteSpace(SongTitle) && TargetPlaylistId > 0)
            {
                // Convert minutes/seconds to total seconds (repository stores Duration as int)
                int totalSeconds = (Minutes * 60) + Seconds;

                // Use repository method that requires userId for ownership checks
                await _playlistRepository.AddSongAsync(TargetPlaylistId, SongTitle, SongArtist, totalSeconds, userId);
            }

            // stay on the same page and show the playlist that was acted on
            return RedirectToPage(new { id = TargetPlaylistId });
        }

        // DELETE A SONG
        public async Task<IActionResult> OnPostDeleteSongAsync(int songId, int playlistId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Repository will ensure ownership before deleting
            await _playlistRepository.DeleteSongAsync(songId, userId);

            // stay on the same playlist after deletion
            return RedirectToPage(new { id = playlistId });
        }

        // DZÇST PLAYLISTI
        public async Task<IActionResult> OnPostDeletePlaylistAsync(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _playlistRepository.DeletePlaylistAsync(id, userId);
            return RedirectToPage();
        }

        // --- NEW: UPDATE SONG INFO ---
        public async Task<IActionResult> OnPostUpdateSongAsync(int songId, int playlistId, string updatedTitle, string updatedArtist)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!string.IsNullOrWhiteSpace(updatedTitle))
            {
                // This calls the repository to save changes. 
                // Ensure your PlaylistRepository has an 'UpdateSongAsync' method.
                await _playlistRepository.UpdateSongAsync(songId, updatedTitle, updatedArtist, userId);
            }

            // Refresh the page on the current playlist
            return RedirectToPage(new { id = playlistId });
        }

    }
}
