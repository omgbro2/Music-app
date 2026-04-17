using Microsoft.Data.Sqlite;

namespace WebApplication2.Models
{
    public class SongRepository
    {
        private readonly string _connectionString;

        public SongRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddSong(int playlistId, string title, string artist, string duration)//TO DO change from void to Async (//Check if we even need this file)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Songs 
                (PlaylistId, Title, Artist, Duration, DateAdded)
                VALUES ($playlistId, $title, $artist, $duration, $dateAdded)";

            command.Parameters.AddWithValue("$playlistId", playlistId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$artist", artist);
            command.Parameters.AddWithValue("$duration", duration);
            command.Parameters.AddWithValue("$dateAdded", DateTime.Now.ToString("yyyy-MM-dd"));

            command.ExecuteNonQuery();
        }

        public List<Song> GetSongs(int playlistId)
        {
            var songs = new List<Song>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Artist, Duration, DateAdded
                FROM Songs
                WHERE PlaylistId = $playlistId";

            command.Parameters.AddWithValue("$playlistId", playlistId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(new Song
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Artist = reader.GetString(2),
                    Duration = reader.GetInt32(3),
                    DateAdded = reader.GetDateTime(4)
                });
            }

            return songs;
        }

        
    }
}