using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class UserCreateViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public string Role { get; set; }

        [Required(ErrorMessage = "Le service est requis")]
        public int ServiceId { get; set; }
    }
}