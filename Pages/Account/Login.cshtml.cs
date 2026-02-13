using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace WebApplication2.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            if (Username == "admin" && Password == "123")
            {
                HttpContext.Session.SetString("Username", Username);
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}
