namespace JconsultGC.Models;

public enum UserRole
{
    Stat,           // Statistiques
    Classement,     // Gestion du classement
    Admin,          // Administrateur
    Indexateur,     // Indexation des documents
    SuperUser,      // Super utilisateur
    ServiceExp,     // Service expéditeur
    Archiviste,     // Gestion des archives
    Viseur,         // Validation/revue
    Signataire      // Signature des documents
}