using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace WebApplication2.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential { get; set; }
        public void OnGet()
        {
            this.Credential = new Credential { UserName = "admin" };
        }

        public void OnPost()
        {
            this.Credential = new Credential { UserName = "admin" };
        }
        //public IActionResult OnPost()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Page();   
        //    }

        //    if (Credential.UserName == "admin" && Credential.Password == "123")
        //    {
        //        return RedirectToPage("/Index");
        //    }

        //    ModelState.AddModelError(string.Empty, "Invalid login attempt");
        //    return Page();
        //}
    }
    public class Credential
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
