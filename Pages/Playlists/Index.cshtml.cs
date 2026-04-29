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

        public async Task<IActionResult> OnGetSongAsync(int SongId)
        {
            var (filedata, contentType) = await _playlistRepository.GetSongData(SongId);
            Response.Headers.AcceptRanges = "bytes";
            Response.Headers.Connection = "keep-alive";
            return new FileStreamResult(new MemoryStream(filedata), contentType);
        }

        public async Task<IActionResult> OnPostAddSongAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!string.IsNullOrWhiteSpace(SongTitle) && TargetPlaylistId > 0)
            {
                if (FileUpload.Length < 10000000) {
                    bool IsMp3 = FileUpload.ContentType == "audio/mpeg";
                    bool IsWav = FileUpload.ContentType == "audio/wav";

                    var fileBuffer = new byte[FileUpload.Length];
                    var file = FileUpload.OpenReadStream().Read(fileBuffer, 0, (int)FileUpload.Length);

                    if (IsMp3){
                        var readResult = TagLibSharp2.Mpeg.Mp3File.Read(fileBuffer.AsSpan());

                        await _playlistRepository.AddSongAsync(TargetPlaylistId, SongTitle, SongArtist, (int)Math.Round(readResult.File.Duration.Value.TotalSeconds), userId, fileBuffer, FileUpload.ContentType);
                    }

                    else if (IsWav)
                    {
                        var readResult = TagLibSharp2.Riff.WavFile.Read(fileBuffer.AsSpan());

                        await _playlistRepository.AddSongAsync(TargetPlaylistId, SongTitle, SongArtist, (int)Math.Round(readResult.File.Properties.Duration.TotalSeconds), userId, fileBuffer, FileUpload.ContentType);
                    }

                    else
                    {
                        return RedirectToPage(new { id = TargetPlaylistId });//TO DO send back an error that file was invalid
                    }
                }
                else
                {
                    return RedirectToPage(new { id = TargetPlaylistId });//TO DO send back an error that file was invalid
                }
            }
            // stay on the same page and show the playlist that was acted on
            return RedirectToPage(new { id = TargetPlaylistId });
        }

        public async Task<IActionResult> OnPostDeleteSongAsync(int songId, int playlistId)
        {
            await _playlistRepository.DeleteSongAsync(songId);
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
        int updatedMinutes,
        int updatedSeconds)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            int totalSeconds = (updatedMinutes * 60) + updatedSeconds;

            await _playlistRepository.UpdateSongAsync(songId, updatedTitle, updatedArtist, totalSeconds, userId);

            return RedirectToPage(new { id = playlistId });
        }

        public async Task<IActionResult> OnPostUpSongAsync(int playlistId, int songId)
        {
            await _playlistRepository.MoveSongUp(songId);
            return RedirectToPage(new { id = playlistId });
        }
        public async Task<IActionResult> OnPostDownSongAsync(int playlistId, int songId)
        {
            await _playlistRepository.MoveSongDown(songId);
            return RedirectToPage(new { id = playlistId });
        }
    }
}
