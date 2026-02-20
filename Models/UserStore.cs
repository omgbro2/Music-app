using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

namespace WebApplication2.Models
{
    public class UserStore : IUserPasswordStore<User>
    {
        private readonly string _connectionString;

        public UserStore(string connectionString)
        {
            _connectionString = connectionString;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    UserName TEXT,
                    NormalizedUserName TEXT,
                    PasswordHash TEXT,
                    DisplayName TEXT,
                    ConcurrencyStamp TEXT,
                    SecurityStamp TEXT
                );";
            command.ExecuteNonQuery();
        }

        public void Dispose() { }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users 
                (Id, UserName, NormalizedUserName, PasswordHash, DisplayName, ConcurrencyStamp, SecurityStamp)
                VALUES ($id, $userName, $normalizedUserName, $passwordHash, $displayName, $concurrencyStamp, $securityStamp)";

            command.Parameters.AddWithValue("$id", user.Id.ToString());
            command.Parameters.AddWithValue("$userName", user.UserName ?? "");
            command.Parameters.AddWithValue("$normalizedUserName", user.NormalizedUserName ?? "");
            command.Parameters.AddWithValue("$passwordHash", user.PasswordHash ?? "");
            command.Parameters.AddWithValue("$displayName", user.DisplayName ?? "");
            command.Parameters.AddWithValue("$concurrencyStamp", user.ConcurrencyStamp ?? "");
            command.Parameters.AddWithValue("$securityStamp", user.SecurityStamp ?? "");

            await command.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Users WHERE NormalizedUserName = $name";
            command.Parameters.AddWithValue("$name", normalizedUserName);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    UserName = reader["UserName"].ToString(),
                    NormalizedUserName = reader["NormalizedUserName"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    DisplayName = reader["DisplayName"].ToString()
                };
            }

            return null;
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user.Id.ToString());

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash != null);

        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult<User?>(null);
    }
}