using System.ComponentModel.DataAnnotations;
namespace JconsultGC.Models;

public class Account
{
    [Key]
    public int AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public decimal Balance { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to User (IdentityUser uses string Id)
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    // Additional properties can be added as needed
}