using System;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using WebApplication2;
using WebApplication2.Models;
using System.Security.Claims;

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

// Register repositories as scoped (per-request)
builder.Services.AddScoped<PlaylistRepository>(sp => new PlaylistRepository(connectionString));
builder.Services.AddScoped<SongRepository>(sp => new SongRepository(connectionString));

// Custom User Store
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

    -- Users: use TEXT for GUID id (matches MyUserStore)
    CREATE TABLE IF NOT EXISTS Users (
        Id TEXT PRIMARY KEY,
        UserName TEXT,
        NormalizedUserName TEXT,
        Email TEXT,
        NormalizedEmail TEXT,
        PasswordHash TEXT,
        ConcurrencyStamp TEXT,
        SecurityStamp TEXT
    );

    -- Playlists: store UserId as TEXT (GUID)
    CREATE TABLE IF NOT EXISTS Playlists (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId TEXT NOT NULL,
        Name TEXT NOT NULL,
        DateCreated TEXT NOT NULL
    );

    -- Songs: Duration as INTEGER
    CREATE TABLE IF NOT EXISTS Songs (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PlaylistId INTEGER NOT NULL,
        Title TEXT NOT NULL,
        Artist TEXT NOT NULL,
        Duration INTEGER NOT NULL,
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

// Root: send authenticated users to Playlists, otherwise to Login
app.MapGet("/", async context =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        context.Response.Redirect("/Playlists/Index");
    }
    else
    {
        context.Response.Redirect("/Account/Login");
    }
    await Task.CompletedTask;
});

app.MapRazorPages();

app.Run();