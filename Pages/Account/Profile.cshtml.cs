using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication2.Models;

public class ProfileModel : PageModel
{
    [BindProperty]
    public string Bio { get; set; }

    public string Username { get; set; }

    public void OnGet()
    {
        Username = User.Identity.Name;

        // Load bio from database here
    }

    public IActionResult OnPost()
    {
        // Save bio to database here
        return RedirectToPage();
    }
}
