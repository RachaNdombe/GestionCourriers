using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class Annotation
{
    [Key]
    public int Id { get; set; }

    public string? Texte { get; set; }

    public int? CourrierId { get; set; }
    public virtual Courrier? Courrier { get; set; }

    public int? AuteurUserId { get; set; }
    public virtual User? Auteur { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
