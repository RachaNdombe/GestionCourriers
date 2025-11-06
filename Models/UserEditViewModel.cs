using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public string Role { get; set; }
    }
}