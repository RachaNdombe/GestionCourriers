using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models;

public class Correspondant
{
	[Key]
	public int Id { get; set; }

	[MaxLength(200)]
	public string? Nom { get; set; }

	[MaxLength(200)]
	public string? Societe { get; set; }

	public string? Adresse { get; set; }

	[Phone]
	public string? Telephone { get; set; }

	[EmailAddress]
	public string? Email { get; set; }

	public virtual ICollection<Courrier>? Courriers { get; set; }
}