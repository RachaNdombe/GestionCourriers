using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class Service
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est requis.")]
    [MaxLength(150)]
    public string? Nom { get; set; }

    [Required(ErrorMessage = "Le code est requis.")]
    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? Responsable { get; set; }

    public bool Actif { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual ICollection<User>? Users { get; set; }
    public virtual ICollection<Courrier>? Courriers { get; set; }
}
