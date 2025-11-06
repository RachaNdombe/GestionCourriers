using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class User : IdentityUser
{
    [Required]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    public string Societe { get; set; } = string.Empty;

    [Required]
    public string Telephone { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<Courrier>? Courriers { get; set; }
    public virtual ICollection<Annotation>? Annotations { get; set; }
    public virtual ICollection<LoginLog>? LoginLogs { get; set; }
    public virtual ICollection<ActiveSession>? ActiveSessions { get; set; }
    public virtual ICollection<Account>? Accounts { get; set; }
    
    // Relations avec le service
    public int? ServiceId { get; set; }
    public virtual Service? Service { get; set; }
}

