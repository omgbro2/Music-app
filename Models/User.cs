using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class User : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;

        // Add these two lines:
        public string AboutMe { get; set; } = "No info provided.";
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
