using Microsoft.Data.Sqlite;

public static class DatabaseHelper
{
    private static string connectionString = "Data Source=app.db";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(connectionString);
    }
}