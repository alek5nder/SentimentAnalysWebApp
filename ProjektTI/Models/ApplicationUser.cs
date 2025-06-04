using Microsoft.AspNetCore.Identity;

namespace ProjektTI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsPremium { get; set; } = false; // Domyślnie użytkownik nie jest premium
    }
}
