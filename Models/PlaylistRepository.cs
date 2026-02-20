using Microsoft.Data.Sqlite;

namespace WebApplication2.Models
{
    public class PlaylistRepository
    {
        private readonly string _connectionString;

        public PlaylistRepository(string connectionString)
        {
            _connectionString = connectionString;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Playlists (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    DateCreated TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Songs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlaylistId INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Artist TEXT NOT NULL,
                    DurationSeconds INTEGER NOT NULL,
                    DateAdded TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        public async Task CreatePlaylistAsync(Guid userId, string name)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Playlists (UserId, Name, DateCreated)
                VALUES ($userId, $name, $date)";

            command.Parameters.AddWithValue("$userId", userId.ToString());
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }

        public async Task AddSongAsync(int playlistId, string title, string artist, int duration)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Songs (PlaylistId, Title, Artist, DurationSeconds, DateAdded)
                VALUES ($playlistId, $title, $artist, $duration, $date)";

            command.Parameters.AddWithValue("$playlistId", playlistId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$artist", artist);
            command.Parameters.AddWithValue("$duration", duration);
            command.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }
    }
}