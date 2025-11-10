using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class ServiceOption
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
    }

    public class LookupOption
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
    }

    public class CourrierCreateViewModel
    {
        [Display(Name = "N° d'ordre")]
        public string? OrdreNumero { get; set; }

        [Display(Name = "N° de registre")]
        public string? RegistreNumero { get; set; }

        [MaxLength(100)]
        [Display(Name = "Référence")]
        public string? ReferenceNumero { get; set; }

        public string? Objet { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de réception")]
        public DateTime? DateReception { get; set; }

        [Display(Name = "Heure de réception")]
        public TimeSpan? HeureRecu { get; set; }

        [Display(Name = "Heure de réception (texte)")]
        public string? HeureRecuText { get; set; }

        [Display(Name = "Nature")]
        public int? NatureCourrierId { get; set; }

        [Display(Name = "Service concerné")]
        public string? ServiceConcerne { get; set; }

        [Display(Name = "Dossier de classement")]
        public int? DossierClassementId { get; set; }

        [Display(Name = "Utilisateurs en copie")]
        public string? UtilisateursEnCopie { get; set; }

        [Display(Name = "Type de dossier")]
        public int? TypeDossierId { get; set; }

        public int? CorrespondantId { get; set; }

        public int? ServiceId { get; set; }

        public int? CategorieCourrierId { get; set; }

        // If user wants to add a new category on the fly
        public string? NewCategorie { get; set; }

        public int? ModeEnvoiId { get; set; }

        public int? UserId {get; set; }

        public ConfidentialiteLevel Confidentialite { get; set; } = ConfidentialiteLevel.Public;

        public PrioriteLevel Priorite { get; set; } = PrioriteLevel.Normal;

        // Uploaded files
        public IFormFileCollection? Attachments { get; set; }

        // Whether the user has viewed/previewed the files before submitting
        public bool VisualizedBeforeSave { get; set; } = false;

        // Dropdown lists
        public List<ServiceOption> AvailableServices { get; set; } = new();
        public List<LookupOption> AvailableCategories { get; set; } = new();
        public List<LookupOption> AvailableNatures { get; set; } = new();
        public List<LookupOption> AvailableModesEnvoi { get; set; } = new();
        public List<LookupOption> AvailableCorrespondants { get; set; } = new();
        public List<LookupOption> AvailableDossiers { get; set; } = new();
        public List<LookupOption> AvailableTypeDossiers { get; set; } = new();
    }
}