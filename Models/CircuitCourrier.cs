using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class CircuitCourrier
{
    [Key]
    public int Id { get; set; }

    [MaxLength(150)]
    public string? Nom { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<CircuitEtape>? Etapes { get; set; }
}
