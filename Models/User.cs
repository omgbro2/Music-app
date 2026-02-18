namespace WebApplication2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // vienkāršs piemērs, vēlāk var hash
    }
}
