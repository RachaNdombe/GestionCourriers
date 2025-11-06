using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class DossierClassement
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Titre { get; set; }

    public string? Description { get; set; }

    public int? TypeDossierId { get; set; }
    public virtual TypeDossier? TypeDossier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Courrier>? Courriers { get; set; }
}
