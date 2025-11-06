using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class Signature
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? NomSignataire { get; set; }

    [MaxLength(150)]
    public string? Titre { get; set; }

    public byte[]? SignatureImage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
