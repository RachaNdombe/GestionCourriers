using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models
{
    public class Signature
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        [StringLength(255)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Chemin vers le fichier de signature numérique
        [StringLength(500)]
        public string? CheminSignature { get; set; }

        // Type de signature: "numerique" ou "manuelle"
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "manuelle";

        public bool EstParDefaut { get; set; } = false;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }
    }

    public class ModificationCourrier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourrierId { get; set; }

        [ForeignKey("CourrierId")]
        public virtual Courrier? Courrier { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public DateTime DateModification { get; set; } = DateTime.Now;

        // Champ modifié (ex: "Note", "Objet", etc.)
        [Required]
        [StringLength(100)]
        public string ChampModifie { get; set; } = string.Empty;

        // Ancienne valeur
        public string? AncienneValeur { get; set; }

        // Nouvelle valeur
        public string? NouvelleValeur { get; set; }

        [StringLength(500)]
        public string? RaisonModification { get; set; }
    }
}
