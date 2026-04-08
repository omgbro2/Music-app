using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebApplication2.Models;

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

        [BindProperty]
        public IFormFile FileUpload { get; set; }

        public List<Playlist> Playlists { get; set; } = new();

        public Playlist CurrentPlaylist { get; set; }

        [BindProperty]
        public string PlaylistName { get; set; }

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


        // IELÂDÇT PLAYLISTES
        // Accept optional id to select a playlist when user clicks on the left side
        public async Task OnGetAsync(int? id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));//TO DO fix this for better 
            Playlists = await _playlistRepository.GetPlaylistsByUserAsync(userId);

            // Choose the requested playlist if present, otherwise the first one
            CurrentPlaylist = id.HasValue
                ? Playlists.FirstOrDefault(p => p.Id == id.Value) : Playlists.FirstOrDefault();

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






                Console.WriteLine(FileUpload.ContentType.ToString());
                Console.WriteLine(FileUpload.GetType());

               await _playlistRepository.AddSongAsync(TargetPlaylistId, SongTitle, SongArtist, 420, userId);//TO DO add duration reading to audio file
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
        public async Task<IActionResult> OnPostUpdateSongAsync(
        int songId,
        int playlistId,
        string updatedTitle,
        string updatedArtist,
        int updatedMinutes,  // <--- Must match name="updatedMinutes"
        int updatedSeconds)  // <--- Must match name="updatedSeconds"
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // CRITICAL: Calculate the total seconds here!
            int totalSeconds = (updatedMinutes * 60) + updatedSeconds;

            // Pass the NEW totalSeconds variable to the repository
            await _playlistRepository.UpdateSongAsync(songId, updatedTitle, updatedArtist, totalSeconds, userId);

            return RedirectToPage(new { id = playlistId });
        }


    }
}
