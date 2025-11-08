using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class ModeEnvoi
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Libelle { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool Actif { get; set; } = true;

    public int Ordre { get; set; } = 0;
}
