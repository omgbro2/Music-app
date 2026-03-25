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

        public ProfileModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public UserInput UsernameData { get; set; } = new();

        public class UserInput
        {
            [Required]
            public string DisplayName { get; set; } = string.Empty;
            public string AboutMe { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UsernameData.DisplayName = user.UserName ?? "";
                UsernameData.AboutMe = user.AboutMe ?? "";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            // PIEŠĶIRAM VĒRTĪBAS MANUĀLI
            user.UserName = UsernameData.DisplayName;
            user.DisplayName = UsernameData.DisplayName;
            user.AboutMe = UsernameData.AboutMe;

            // NOŅEMAM VALIDĀCIJAS KĻŪDAS (lai nekas nebloķētu)
            ModelState.Clear();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToPage();
            }

            return Page();
        }
    }
}
