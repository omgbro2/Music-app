using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using WebApplication2.Models;
using System.Security.Claims;

namespace WebApplication2
{
    public class MyUserStore :
        IUserPasswordStore<User>
    {

        private readonly string _connectionString;

        public MyUserStore(string connectionString)
        {
            _connectionString = connectionString;
            // Ensure the database and Users table exist
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    UserName TEXT,
                    NormalizedUserName TEXT,
                    PasswordHash TEXT,
                    ConcurrencyStamp TEXT,
                    SecurityStamp TEXT
                )";
            command.ExecuteNonQuery();

            connection.Close();
        }

        private static void FillParams(SqliteCommand command, User user)
        {
            command.Parameters.AddRange(new[] {
                    new SqliteParameter( "$id", user.Id.ToString() ),
                    new SqliteParameter( "$userName", user.UserName ?? string.Empty ),
                    new SqliteParameter( "$normalizedUserName", user.NormalizedUserName ?? string.Empty ),
                    new SqliteParameter( "$passwordHash", user.PasswordHash ?? string.Empty ),
                    new SqliteParameter( "$concurrencyStamp", user.ConcurrencyStamp ?? string.Empty ),
                    new SqliteParameter( "$securityStamp", user.SecurityStamp ?? string.Empty )
            });
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Users (Id, UserName, NormalizedUserName, PasswordHash, ConcurrencyStamp, SecurityStamp)
                VALUES ($id, $userName, $normalizedUserName, $passwordHash, $concurrencyStamp, $securityStamp)";
                FillParams(command, user);
                await command.ExecuteNonQueryAsync(cancellationToken);
                return IdentityResult.Success;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }



        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            using SqliteConnection _connection = new SqliteConnection(_connectionString);
            await _connection.OpenAsync(cancellationToken);
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Users
                WHERE Id = $id";
            command.Parameters.AddWithValue("$id", user.Id.ToString());
            await command.ExecuteNonQueryAsync(cancellationToken);
            await _connection.CloseAsync();
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }


        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Id, UserName, NormalizedUserName, PasswordHash, ConcurrencyStamp, SecurityStamp
                FROM Users
                WHERE Id = $id";
                command.Parameters.AddWithValue("$id", userId);
                using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    User user = new User
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        UserName = reader.GetString(1),
                        NormalizedUserName = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        ConcurrencyStamp = reader.GetString(4),
                        SecurityStamp = reader.GetString(5)
                    };
                    return user;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return null;
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Id, UserName, NormalizedUserName, PasswordHash, ConcurrencyStamp, SecurityStamp
                FROM Users
                WHERE NormalizedUserName = $normalizedUserName";
                command.Parameters.AddWithValue("$normalizedUserName", normalizedUserName);
                using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    User user = new User
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        UserName = reader.GetString(1),
                        NormalizedUserName = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        ConcurrencyStamp = reader.GetString(4),
                        SecurityStamp = reader.GetString(5)
                    };
                    return user;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return null;

        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Users
                    SET UserName = $userName,
                        NormalizedUserName = $normalizedUserName,
                        PasswordHash = $passwordHash,
                        ConcurrencyStamp = $concurrencyStamp,
                        SecurityStamp = $securityStamp
                    WHERE Id = $id";
                FillParams(command, user);
                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    return IdentityResult.Success;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return IdentityResult.Failed();
        }



    }
}
