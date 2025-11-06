using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class Rapport
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Titre { get; set; }

    public string? Description { get; set; }

    public DateTime DateGeneration { get; set; } = DateTime.UtcNow;

    public string? FichierPath { get; set; }
}
