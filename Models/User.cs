using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class User : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}