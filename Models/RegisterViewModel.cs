using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
    [Display(Name = "Nom d'utilisateur")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La société est requise")]
    public string Societe { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone est requis")]
    [Phone(ErrorMessage = "Format de téléphone invalide")]
    public string Telephone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le rôle est requis")]
    [Display(Name = "Rôle")]
    public UserRole Role { get; set; }

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [StringLength(100, ErrorMessage = "Le {0} doit avoir au moins {2} caractères.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public int? ServiceId { get; set; }
}