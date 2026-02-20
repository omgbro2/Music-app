

using Microsoft.AspNetCore.Identity;
using WebApplication2;
using WebApplication2.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorPages();


builder.Services.AddAuthentication("Identity.Application")
       .AddCookie("Identity.Application", options =>
       {
           options.LoginPath = "/user/login";
           options.LogoutPath = "/user/logout";
           options.Cookie.Name = "Identity.Application";
       });

builder.Services.AddAuthorization();


builder.Services.AddScoped<IUserStore<User>, MyUserStore>(serviceProvider =>
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection") ?? "Data Source=sampleapp.db";
    return new MyUserStore(connectionString);
});
builder.Services.AddIdentityCore<User>(options =>
{
    // Configure password rules if you like
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddSignInManager()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
