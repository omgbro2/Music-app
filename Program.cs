using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using WebApplication2;
using WebApplication2.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------

builder.Services.AddRazorPages();

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "Identity.Application";
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ✅ Register repositories (NO EF)
builder.Services.AddSingleton<PlaylistRepository>(
    new PlaylistRepository(connectionString));

builder.Services.AddSingleton<SongRepository>(
    new SongRepository(connectionString));

// ✅ Custom User Store
builder.Services.AddScoped<IUserStore<User>>(
    serviceProvider => new MyUserStore(connectionString));

builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddSignInManager()
.AddDefaultTokenProviders();


// --------------------
// CREATE TABLES (Auto Run)
// --------------------

void CreateTables(string connString)
{
    using var connection = new SqliteConnection(connString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"

    CREATE TABLE IF NOT EXISTS Users (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserName TEXT,
        NormalizedUserName TEXT,
        Email TEXT,
        NormalizedEmail TEXT,
        PasswordHash TEXT
    );

    CREATE TABLE IF NOT EXISTS Playlists (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER NOT NULL,
        Name TEXT NOT NULL,
        DateCreated TEXT NOT NULL
    );

    CREATE TABLE IF NOT EXISTS Songs (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PlaylistId INTEGER NOT NULL,
        Title TEXT NOT NULL,
        Artist TEXT NOT NULL,
        Duration TEXT NOT NULL,
        DateAdded TEXT NOT NULL
    );

    ";

    command.ExecuteNonQuery();
}

CreateTables(connectionString);

// --------------------
// Build App
// --------------------

var app = builder.Build();

// --------------------
// Middleware
// --------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // IMPORTANT
app.UseAuthorization();

app.MapRazorPages();

app.Run();