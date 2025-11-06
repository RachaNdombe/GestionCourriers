using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class LoginWith2faViewModel
{
    [Required(ErrorMessage = "Le code d'authentification est requis")]
    [StringLength(7, ErrorMessage = "Le code doit faire entre {2} et {1} caractères.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Code d'authentification")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Display(Name = "Mémoriser cet appareil")]
    public bool RememberMachine { get; set; }

    public bool RememberMe { get; set; }
}