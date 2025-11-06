using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models;

public class Courrier
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "N° d'ordre")]
    public string? OrdreNumero { get; set; }

    [Display(Name = "N° de registre")]
    public string? RegistreNumero { get; set; }

    [MaxLength(100)]
    [Display(Name = "Référence")]
    public string? ReferenceNumero { get; set; }

    public string? Objet { get; set; }

    public DateTime DateEnregistrement { get; set; } = DateTime.UtcNow;

    [Display(Name = "Date de réception")]
    public DateTime? DateReception { get; set; }

    [Display(Name = "Heure de réception")]
    public TimeSpan? HeureRecu { get; set; }

    public DateTime? DateSortie { get; set; }

    [Display(Name = "Nature")]
    public string? Nature { get; set; }

    [Display(Name = "Service concerné")]
    public string? ServiceConcerne { get; set; }

    [Display(Name = "Dossier de classement")]
    public string? DossierDeClassement { get; set; }

    [Display(Name = "Utilisateurs en copie")]
    public string? UtilisateursEnCopie { get; set; }

    [Display(Name = "Type de dossier")]
    public int? TypeDossierId { get; set; }
    public virtual TypeDossier? TypeDossier { get; set; }

    // Category
    public int? CategorieCourrierId { get; set; }
    public virtual CategorieCourrier? CategorieCourrier { get; set; }
    public int? CorrespondantId { get; set; }
    public virtual Correspondant? Correspondant { get; set; }

    public int? ServiceId { get; set; }
    public virtual Service? Service { get; set; }

    public bool IsArchive { get; set; } = false;

    public string? Statut { get; set; }

    // Who created / indexed this courrier
    public string? CreatedById { get; set; }
    public virtual User? CreatedBy { get; set; }

    public virtual ICollection<Document>? Documents { get; set; }
    public virtual ICollection<Annotation>? Annotations { get; set; }

    // Mode d'envoi
    public int? ModeEnvoiId { get; set; }
    public virtual ModeEnvoi? ModeEnvoi { get; set; }

    // Confidentialité (4 niveaux)
    public ConfidentialiteLevel Confidentialite { get; set; } = ConfidentialiteLevel.Public;

    // Priorité (4 niveaux)
    public PrioriteLevel Priorite { get; set; } = PrioriteLevel.Normal;
}

public enum ConfidentialiteLevel
{
    Public = 0,
    Interne = 1,
    Confidentiel = 2,
    TresConfidentiel = 3
}

public enum PrioriteLevel
{
    Basse = 0,
    Normal = 1,
    Haute = 2,
    Urgent = 3
}
