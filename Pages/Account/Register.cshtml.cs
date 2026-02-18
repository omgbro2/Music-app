using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;

namespace WebApplication2.Pages.Account
{
    public class RegisterModel : PageModel
    {
        //private readonly AppDbContext _context;

        //public RegisterModel(AppDbContext context)
        //{
        //    _context = context;
        //}

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            //if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            //{
            //    return Page();
            //}

            //// Pârbauda, vai jau eksistç
            //if (_context.Users.Any(u => u.Username == Username))
            //{
            //    ModelState.AddModelError(string.Empty, "Username already exists!");
            //    return Page();
            //}

            //var user = new User
            //{
            //    Username = Username,
            //    Password = Password // vçlâk var hash
            //};

            //_context.Users.Add(user);
            //_context.SaveChanges();

            return RedirectToPage("/Account/Login");
        }
    }
}
