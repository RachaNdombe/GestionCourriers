using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class CategorieCourrier
{
    [Key]
    public int Id { get; set; }

    [MaxLength(150)]
    public string? Nom { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    public virtual ICollection<Courrier>? Courriers { get; set; }
}
