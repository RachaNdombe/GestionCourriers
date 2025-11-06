using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class ParametreSociete
{
    [Key]
    public int Id { get; set; }

    public string? NomSociete { get; set; }

    public string? Adresse { get; set; }

    public string? Telephone { get; set; }

    public string? Email { get; set; }

    public string? LogoPath { get; set; }
}
