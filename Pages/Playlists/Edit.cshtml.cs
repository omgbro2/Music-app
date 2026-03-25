using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Pages.Playlists
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly PlaylistRepository _playlistRepository;

        public EditModel(PlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Name { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id, userId);
            if (playlist == null)
            {
                return NotFound();
            }

            Id = playlist.Id;
            Name = playlist.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "Playlist name cannot be empty.");
                return Page();
            }

            await _playlistRepository.EditPlaylistAsync(Id, Name, userId);

            return RedirectToPage("/Playlists/Index");
        }   
    }
}