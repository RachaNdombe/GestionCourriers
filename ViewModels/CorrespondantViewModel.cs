using System.ComponentModel.DataAnnotations;


namespace JconsultGC.ViewModels
{
    public class CorrespondantViewModel
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "La société est obligatoire")]
        [Display(Name = "Société")]
        public string Societe { get; set; }

        [Display(Name = "Adresse")]
        public string Adresse { get; set; }

        [Display(Name = "Téléphone")]
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string Telephone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; }
    }
}