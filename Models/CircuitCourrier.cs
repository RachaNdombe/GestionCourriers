using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class CircuitCourrier
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string NomActions { get; set; } = string.Empty;

    public int? ServiceId { get; set; }
    public virtual Service? Service { get; set; }

    public string? UsersCircuitId { get; set; }
    public virtual User? UsersCircuit { get; set; }

    public string? Commentaire { get; set; }

    public int TypeCircuit { get; set; } // 1 = Courrier Entrant, 2 = Courrier Sortant

    public int OrdreExecution { get; set; }

    public bool Obligatoire { get; set; }

    public bool Actif { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual ICollection<CircuitEtape>? Etapes { get; set; }
}
