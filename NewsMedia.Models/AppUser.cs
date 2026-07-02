using Microsoft.AspNetCore.Identity;

namespace NewsMedia.Models
{
    public class AppUser : IdentityUser
    {
        // Id, UserName, Email ya vienen de IdentityUser
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}