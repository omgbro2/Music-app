using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Account
{
    public class RegisterModel : PageModel
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        [BindProperty]
        public RegisterInput Register { get; set; } = new();


        public string? ErrorMessage { get; set; }

        public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public class RegisterInput
        {
            [Required]
            public string UserName { get; set; } = string.Empty;


            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {


            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = Register.UserName,
            };

            var result = await _userManager.CreateAsync(user, Register.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("/");
            }

            ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Page();
        }
    }
}