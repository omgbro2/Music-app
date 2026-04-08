using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LoginModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public LoginInput Login { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class LoginInput
        {
            [Required]
            public string UserName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }


        public async Task<IActionResult> OnPostLoginAsync()
        {
            ModelState.Clear();
            TryValidateModel(Login, nameof(Login));
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Login.UserName,
                Login.Password,
                Login.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToPage("/Playlists/Index");

            ErrorMessage = "Invalid login attempt.";
            return Page();
        }

    }

}