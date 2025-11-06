using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class CircuitEtape
{
    [Key]
    public int Id { get; set; }

    public int CircuitCourrierId { get; set; }
    public virtual CircuitCourrier? CircuitCourrier { get; set; }

    [MaxLength(150)]
    public string? NomEtape { get; set; }

    public int Ordre { get; set; }

    public int? ResponsableUserId { get; set; }
}
