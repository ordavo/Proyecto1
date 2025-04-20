using Microsoft.AspNetCore.Identity;

namespace ManejoPresupuesto.Models
{
    public class Usuario : IdentityUser<int>
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PasswordHash { get; set; }
    }
}
