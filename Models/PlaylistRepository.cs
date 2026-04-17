using Microsoft.Data.Sqlite;
using TagLibSharp2.Core;


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
                    Duration BLOB NOT NULL,
                    DateAdded TEXT NOT NULL,
                    Audio BLOB NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        // CREATE
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

        // READ ALL (safe)
        public async Task<List<Playlist>> GetPlaylistsByUserAsync(Guid userId)
        {
            var playlists = new List<Playlist>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, DateCreated
                FROM Playlists
                WHERE UserId = $userId";

            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                playlists.Add(new Playlist
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    DateCreated = reader.GetString(2)
                });
            }

            // Load songs for each playlist so Razor can access playlist.Songs
            foreach (var p in playlists)
            {
                p.Songs = await GetSongsByPlaylistAsync(p.Id, userId);
            }

            return playlists;
        }

        // READ SINGLE (safe)
        public async Task<Playlist?> GetPlaylistByIdAsync(int id, Guid userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, DateCreated
                FROM Playlists
                WHERE Id = $id AND UserId = $userId";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Playlist
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    DateCreated = reader.GetString(2)
                };
            }

            return null;
        }

        // UPDATE (safe)
        public async Task EditPlaylistAsync(int id, string newName, Guid userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Playlists
                SET Name = $name
                WHERE Id = $id AND UserId = $userId";

            command.Parameters.AddWithValue("$name", newName);
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$userId", userId.ToString());

            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        // DELETE (safe)
        public async Task DeletePlaylistAsync(int id, Guid userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Delete songs first (only for playlists owned by user)
            var cmd1 = connection.CreateCommand();
            cmd1.CommandText = @"
                DELETE FROM Songs
                WHERE PlaylistId = $id";
            cmd1.Parameters.AddWithValue("$id", id);
            await cmd1.ExecuteNonQueryAsync();

            // Delete playlist safely
            var cmd2 = connection.CreateCommand();
            cmd2.CommandText = @"
                DELETE FROM Playlists
                WHERE Id = $id AND UserId = $userId";

            cmd2.Parameters.AddWithValue("$id", id);
            cmd2.Parameters.AddWithValue("$userId", userId.ToString());

            await cmd2.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        // SONGS (safe via playlist ownership)
        public async Task<List<Song>> GetSongsByPlaylistAsync(int playlistId, Guid userId)
        {
            var songs = new List<Song>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT s.Id, s.Title, s.Artist, s.Duration, s.DateAdded
                FROM Songs s
                JOIN Playlists p ON s.PlaylistId = p.Id
                WHERE s.PlaylistId = $playlistId AND p.UserId = $userId";

            command.Parameters.AddWithValue("$playlistId", playlistId);
            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
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
            await connection.CloseAsync();
            return songs;
        }

        public async Task AddSongAsync(int playlistId, string title, string artist, int duration, Guid userId, byte[] audio)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Ensure playlist belongs to user
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = @"
                SELECT COUNT(*)
                FROM Playlists
                WHERE Id = $playlistId AND UserId = $userId";

            checkCmd.Parameters.AddWithValue("$playlistId", playlistId);
            checkCmd.Parameters.AddWithValue("$userId", userId.ToString());

            var exists = (long)await checkCmd.ExecuteScalarAsync();
            if (exists == 0) return;

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Songs (PlaylistId, Title, Artist, Duration, DateAdded, Audio)
                VALUES ($playlistId, $title, $artist, $duration, $date, $audio)";

            command.Parameters.AddWithValue("$playlistId", playlistId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$artist", artist);
            command.Parameters.AddWithValue("$duration", duration);
            command.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("$audio", audio);

            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        // Delete a single song but only if it belongs to a playlist owned by the user
        public async Task DeleteSongAsync(int songId, Guid userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Songs
                WHERE Id = $songId
                  AND PlaylistId IN (
                      SELECT Id FROM Playlists WHERE UserId = $userId
                  );";

            command.Parameters.AddWithValue("$songId", songId);
            command.Parameters.AddWithValue("$userId", userId.ToString());

            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
        public async Task UpdateSongAsync(int songId, string title, string artist, int duration, Guid userId)//TO DO double check this function if it works
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE Songs
            SET Title = $title, 
            Artist = $artist, 
            Duration = $duration   -- <--- DID YOU ADD THIS LINE?
            WHERE Id = $id AND PlaylistId IN (
            SELECT Id FROM Playlists WHERE UserId = $userId
            )";

            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$artist", artist);
            command.Parameters.AddWithValue("$duration", duration); // <--- AND THIS ONE?
            command.Parameters.AddWithValue("$id", songId);
            command.Parameters.AddWithValue("$userId", userId.ToString());

            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

    }
}