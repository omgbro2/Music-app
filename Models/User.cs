using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class User : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;
        public string AboutMe { get; set; } = string.Empty; 
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

}
