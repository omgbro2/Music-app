using Microsoft.Data.Sqlite;
using TagLibSharp2.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


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
                    Audio BLOB NOT NULL,
                    ContentType TEXT NOT NULL,
                    PreviouSong INTEGER,
                    NextSong INTEGER
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
                SELECT s.Id, s.Title, s.Artist, s.Duration, s.DateAdded, s.NextSong
                FROM Songs s
                JOIN Playlists p ON s.PlaylistId = p.Id
                WHERE s.PlaylistId = $playlistId AND p.UserId = $userId
                ORDER BY CASE WHEN s.NextSong IS NULL THEN 1 ELSE 0 END,
                s.NextSong ASC";

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

        public async Task AddSongAsync(int playlistId, string title, string artist, int duration, Guid userId, byte[] audio, string ContentType)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using (var transaction = connection.BeginTransaction())
            {

                // Ensure playlist belongs to user
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM Playlists
                    WHERE Id = $playlistId AND UserId = $userId
                    ";

                checkCmd.Parameters.AddWithValue("$playlistId", playlistId);
                checkCmd.Parameters.AddWithValue("$userId", userId.ToString());
                var exists = (long)await checkCmd.ExecuteScalarAsync();
                if (exists == 0) return;

                //find previous song
                var songfind = connection.CreateCommand();
                songfind.CommandText = @"
                SELECT Id FROM Songs
                WHERE PlaylistId = $playlistId AND NextSong IS NULL
                ";
                songfind.Parameters.AddWithValue("$playlistId", playlistId);
                var PrevSongId = await songfind.ExecuteScalarAsync();
                
                //makes new song
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Songs (PlaylistId, Title, Artist, Duration, DateAdded, Audio, ContentType, PreviouSong)
                    VALUES ($playlistId, $title, $artist, $duration, $date, $audio, $contenttype, $previousong)
                    RETURNING Id
                    ";
                command.Parameters.AddWithValue("$playlistId", playlistId);
                command.Parameters.AddWithValue("$title", title);
                command.Parameters.AddWithValue("$artist", artist);
                command.Parameters.AddWithValue("$duration", duration);
                command.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("$audio", audio);
                command.Parameters.AddWithValue("$contenttype", ContentType);
                command.Parameters.AddWithValue("$previousong", PrevSongId ?? DBNull.Value);
                var NextSong = await command.ExecuteScalarAsync();

                if (PrevSongId != null)
                {
                    var Update = connection.CreateCommand();
                    Update.CommandText = @"
                        UPDATE Songs
                        SET NextSong = $nextsong
                        WHERE Id = $id
                        ";
                    Update.Parameters.AddWithValue("$nextsong", NextSong);
                    Update.Parameters.AddWithValue("$id", PrevSongId);
                    await Update.ExecuteNonQueryAsync();
                }
                transaction.Commit();
            }
            await connection.CloseAsync();
        }

        public async Task DeleteSongAsync(int songId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            //find previous song
            var GetData = connection.CreateCommand();
            GetData.CommandText = @"
                SELECT PreviouSong FROM Songs
                WHERE Id = $songId
                ";
            GetData.Parameters.AddWithValue("$songId", songId);
            var PrevSongId = await GetData.ExecuteScalarAsync();

            //find next song
            var heligop = connection.CreateCommand();
            heligop.CommandText = @"
                SELECT NextSong FROM Songs
                WHERE Id = $songId
                ";
            heligop.Parameters.AddWithValue("$songId", songId);
            var NextSongId = await heligop.ExecuteScalarAsync();

            //update previous song
            var helipop = connection.CreateCommand();
            helipop.CommandText = @"
                UPDATE Songs
                SET NextSong = $nextsong
                WHERE Id = $prevsong
                ";
            helipop.Parameters.AddWithValue("$prevsong", PrevSongId);
            helipop.Parameters.AddWithValue("$nextsong", NextSongId ?? DBNull.Value);
            await helipop.ExecuteNonQueryAsync();

            //update next song
            var helipog = connection.CreateCommand();
            helipog.CommandText = @"
                UPDATE Songs
                SET PreviouSong = $prevsong
                WHERE Id = $nextsong
                ";
            helipog.Parameters.AddWithValue("$prevsong", PrevSongId ?? DBNull.Value);
            helipog.Parameters.AddWithValue("$nextsong", NextSongId);
            await helipog.ExecuteNonQueryAsync();


            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Songs
                WHERE Id = $songId
                ";
            command.Parameters.AddWithValue("$songId", songId);

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
        public async Task<(byte[], string)> GetSongData(int SongID)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = @"
                SELECT Audio, ContentType
                FROM Songs
                WHERE Id = $songId";
            checkCmd.Parameters.AddWithValue("$songId", SongID);
            using SqliteDataReader reader = await checkCmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            byte[] blob = (byte[])reader[0];
            return (blob, reader.GetString(1));
        }
        public async Task MoveSongUp(int songId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var GetData = connection.CreateCommand();
            GetData.CommandText = @"
                SELECT PreviouSong FROM Songs
                WHERE Id = $songId
                ";
            GetData.Parameters.AddWithValue("$songId", songId);
            var PrevSongId = await GetData.ExecuteScalarAsync();

            if (PrevSongId != DBNull.Value)
            {

                var heligop = connection.CreateCommand();
                heligop.CommandText = @"
                    SELECT NextSong FROM Songs
                    WHERE Id = $songId
                    ";
                heligop.Parameters.AddWithValue("$songId", songId);
                var NextSongId = await heligop.ExecuteScalarAsync();

                var lolipop = connection.CreateCommand();
                lolipop.CommandText = @"
                    SELECT PreviouSong FROM Songs
                    WHERE Id = $songId
                    ";
                lolipop.Parameters.AddWithValue("$songId", PrevSongId);
                var PrevSong2Id = await lolipop.ExecuteScalarAsync();

                var sourpop = connection.CreateCommand();
                sourpop.CommandText = @"
                    SELECT NextSong FROM Songs
                    WHERE Id = $songId
                    ";
                sourpop.Parameters.AddWithValue("$songId", PrevSongId);
                var NextSong2Id = await sourpop.ExecuteScalarAsync();

                //update the current song
                var Update = connection.CreateCommand();
                Update.CommandText = @"
                    UPDATE Songs
                    SET PreviouSong = $prevSong2Id,
                    NextSong = $nextSong2Id
                    WHERE Id = $id
                    ";
                Update.Parameters.AddWithValue("$id", songId);
                Update.Parameters.AddWithValue("$prevSong2Id", PrevSong2Id ?? DBNull.Value);
                Update.Parameters.AddWithValue("$nextSong2Id", PrevSongId);
                await Update.ExecuteNonQueryAsync();

                //updates the down song
                var UpdateNew = connection.CreateCommand();
                UpdateNew.CommandText = @"
                    UPDATE Songs
                    SET PreviouSong = $prevSong2Id,
                    NextSong = $nextSong2Id
                    WHERE Id = $id
                    ";
                UpdateNew.Parameters.AddWithValue("$id", PrevSongId);
                UpdateNew.Parameters.AddWithValue("$prevSong2Id", NextSong2Id ?? DBNull.Value);
                UpdateNew.Parameters.AddWithValue("$nextSong2Id", NextSongId);
                await UpdateNew.ExecuteNonQueryAsync();

                //updates up song
                var UpdateOld = connection.CreateCommand();
                UpdateOld.CommandText = @"
                    UPDATE Songs
                    SET PreviouSong = $prevSong2Id
                    WHERE Id = $id
                    ";
                UpdateOld.Parameters.AddWithValue("$id", NextSongId);
                UpdateOld.Parameters.AddWithValue("$prevSong2Id", PrevSongId);
                await UpdateOld.ExecuteNonQueryAsync();

                //updates up up song
                var UpdateOldOld = connection.CreateCommand();
                UpdateOldOld.CommandText = @"
                    UPDATE Songs
                    SET NextSong = $nextSong2Id
                    WHERE Id = $id
                    ";
                UpdateOldOld.Parameters.AddWithValue("$id", PrevSong2Id);
                UpdateOldOld.Parameters.AddWithValue("$nextSong2Id", NextSong2Id);
                await UpdateOldOld.ExecuteNonQueryAsync();
            }
        }
        public async Task MoveSongDown(int songId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var heligop = connection.CreateCommand();
            heligop.CommandText = @"
                SELECT NextSong FROM Songs
                WHERE Id = $songId  
                ";
            heligop.Parameters.AddWithValue("$songId", songId);
            var NextSongId = await heligop.ExecuteScalarAsync();

            if (NextSongId != DBNull.Value)
            {

                var GetData = connection.CreateCommand();
                GetData.CommandText = @"
                    SELECT PreviouSong FROM Songs
                    WHERE Id = $songId
                    ";
                GetData.Parameters.AddWithValue("$songId", songId);
                var PrevSongId = await GetData.ExecuteScalarAsync();

                var lolipop = connection.CreateCommand();
                lolipop.CommandText = @"
                    SELECT PreviouSong FROM Songs
                    WHERE Id = $songId
                    ";
                lolipop.Parameters.AddWithValue("$songId", NextSongId);
                var PrevSong2Id = await lolipop.ExecuteScalarAsync();

                var sourpop = connection.CreateCommand();
                sourpop.CommandText = @"
                    SELECT NextSong FROM Songs
                    WHERE Id = $songId
                    ";
                sourpop.Parameters.AddWithValue("$songId", NextSongId);
                var NextSong2Id = await sourpop.ExecuteScalarAsync();

                var pankuka = connection.CreateCommand();
                pankuka.CommandText = @"
                    SELECT PreviouSong FROM Songs
                    WHERE Id = $songId
                    ";
                pankuka.Parameters.AddWithValue("$songId", NextSongId);
                var PrevSong3Id = await pankuka.ExecuteScalarAsync();

                var bunzi = connection.CreateCommand();
                bunzi.CommandText = @"
                    SELECT NextSong FROM Songs
                    WHERE Id = $songId
                    ";
                bunzi.Parameters.AddWithValue("$songId", NextSongId);
                var NextSong3Id = await bunzi.ExecuteScalarAsync();

                //update current song
                var Update = connection.CreateCommand();
                Update.CommandText = @"
                    UPDATE Songs
                    SET PreviouSong = $prevSong2Id,
                    NextSong = $nextSong2Id
                    WHERE Id = $id
                    ";
                Update.Parameters.AddWithValue("$id", songId);
                Update.Parameters.AddWithValue("$prevSong2Id", NextSongId);
                Update.Parameters.AddWithValue("$nextSong2Id", NextSong2Id ?? DBNull.Value);
                await Update.ExecuteNonQueryAsync();

                //update down song
                var UpdateNew = connection.CreateCommand();
                UpdateNew.CommandText = @"
                    UPDATE Songs
                    SET PreviouSong = $prevSong2Id,
                    NextSong = $nextSong2Id
                    WHERE Id = $id
                    ";
                UpdateNew.Parameters.AddWithValue("$id", NextSongId);
                UpdateNew.Parameters.AddWithValue("$prevSong2Id", PrevSongId ?? DBNull.Value);
                UpdateNew.Parameters.AddWithValue("$nextSong2Id", PrevSong2Id);
                await UpdateNew.ExecuteNonQueryAsync();

                //update down down song
                var UpdateOld = connection.CreateCommand();
                UpdateOld.CommandText = @"
                    UPDATE Songs
                    SET PreviouSong = $prevSong2Id
                    WHERE Id = $id
                    ";
                UpdateOld.Parameters.AddWithValue("$id", NextSong2Id);
                UpdateOld.Parameters.AddWithValue("$prevSong2Id", PrevSong2Id);
                await UpdateOld.ExecuteNonQueryAsync();

                //updates up song
                var UpdateOldOld = connection.CreateCommand();
                UpdateOldOld.CommandText = @"
                    UPDATE Songs
                    SET NextSong = $nextSong2Id
                    WHERE Id = $id
                    ";
                UpdateOldOld.Parameters.AddWithValue("$id", PrevSongId);
                UpdateOldOld.Parameters.AddWithValue("$nextSong2Id", NextSongId);
                await UpdateOldOld.ExecuteNonQueryAsync();
            }
        }

        public async Task GetSongList(int PlaylistId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var getsongs = connection.CreateCommand();
            getsongs.CommandText = @"
                    SELECT NextSong FROM Songs
                    ";
            getsongs.Parameters.AddWithValue("$songId", PlaylistId);
            var songs = await getsongs.ExecuteScalarAsync();
        }
    }
}