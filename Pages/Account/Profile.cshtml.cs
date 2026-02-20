using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Pages.Account
{
    public class ProfileModel : PageModel
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        [BindProperty]
        public UserInput UsernameData { get; set; } = new();

        public class UserInput
        {
            [Required]
            public string DisplayName { get; set; } = string.Empty;
        }

        public ProfileModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            RedirectToPage();
        }

    }
}
