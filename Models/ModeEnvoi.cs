using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class ModeEnvoi
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Nom { get; set; }

    public string? Description { get; set; }
}
