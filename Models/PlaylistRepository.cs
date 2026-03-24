using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    Duration INTEGER NOT NULL,
                    DateAdded TEXT NOT NULL
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

        // READ ALL
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

            return playlists;
        }

        // READ SINGLE
        public async Task<Playlist?> GetPlaylistByIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, DateCreated
                FROM Playlists
                WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

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

        // UPDATE
        public async Task EditPlaylistAsync(int id, string newName)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Playlists
                SET Name = $name
                WHERE Id = $id";
            command.Parameters.AddWithValue("$name", newName);
            command.Parameters.AddWithValue("$id", id);

            await command.ExecuteNonQueryAsync();
        }

        // DELETE
        public async Task DeletePlaylistAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Delete songs first
            var cmd1 = connection.CreateCommand();
            cmd1.CommandText = "DELETE FROM Songs WHERE PlaylistId = $id";
            cmd1.Parameters.AddWithValue("$id", id);
            await cmd1.ExecuteNonQueryAsync();

            // Delete playlist
            var cmd2 = connection.CreateCommand();
            cmd2.CommandText = "DELETE FROM Playlists WHERE Id = $id";
            cmd2.Parameters.AddWithValue("$id", id);
            await cmd2.ExecuteNonQueryAsync();
        }

        // SONGS
        public async Task<List<Song>> GetSongsByPlaylistAsync(int playlistId)
        {
            var songs = new List<Song>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Artist, Duration, DateAdded
                FROM Songs
                WHERE PlaylistId = $playlistId";
            command.Parameters.AddWithValue("$playlistId", playlistId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                songs.Add(new Song
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Artist = reader.GetString(2),
                    Duration = reader.GetInt32(3),
                    DateAdded = reader.GetString(4)
                });
            }

            return songs;
        }

        public async Task AddSongAsync(int playlistId, string title, string artist, int duration)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Songs (PlaylistId, Title, Artist, Duration, DateAdded)
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