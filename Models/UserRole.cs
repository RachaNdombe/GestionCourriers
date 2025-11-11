using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public enum UserRole
{
    [Display(Name = "Statistiques")]
    Stat,           // Statistiques
    
    [Display(Name = "Gestion du classement")]
    Classement,     // Gestion du classement
    
    [Display(Name = "Administrateur")]
    Admin,          // Administrateur
    
    [Display(Name = "Indexation des documents")]
    Indexateur,     // Indexation des documents
    
    [Display(Name = "Super utilisateur")]
    SuperUser,      // Super utilisateur
    
    [Display(Name = "Service expéditeur")]
    ServiceExpéditeur, // Service expéditeur
    
    [Display(Name = "Gestion des archives")]
    Archiviste,     // Gestion des archives
    
    [Display(Name = "Validation/revue")]
    Viseur,         // Validation/revue
    
    [Display(Name = "Signature des documents")]
    Signataire      // Signature des documents
}