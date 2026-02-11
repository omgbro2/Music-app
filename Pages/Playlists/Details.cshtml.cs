using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace WebApplication2.Pages.Playlists
{
    public class DetailsModel : PageModel
    {
        public string PlaylistName { get; set; }
        public List<Song> Songs { get; set; }

        public void OnGet(int id)
        {
            PlaylistName = "Playlist " + id;

            Songs = new List<Song>
            {
                new Song { Title = "Song 1", Artist = "Artist 1", DateAdded = DateTime.Now },
                new Song { Title = "Song 2", Artist = "Artist 2", DateAdded = DateTime.Now }
            };
        }
    }

    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
