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
        public async Task OnGetAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);
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

            return RedirectToPage();
        }

        // DZÇST PLAYLISTI
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _playlistRepository.DeletePlaylistAsync(id, userId);
            return RedirectToPage();
        }
    }
}
