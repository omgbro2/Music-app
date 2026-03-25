using Microsoft.Data.Sqlite;
using System.Security.Claims;

public static class DatabaseHelper
{
    private static string connectionString = "Data Source=app.db";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(connectionString);
    }
}