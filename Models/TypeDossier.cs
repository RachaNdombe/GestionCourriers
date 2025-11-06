using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class TypeDossier
{
    [Key]
    public int Id { get; set; }

    [MaxLength(150)]
    public string? Libelle { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<DossierClassement>? Dossiers { get; set; }
}
