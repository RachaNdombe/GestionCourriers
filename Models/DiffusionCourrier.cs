using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models
{
    public class DiffusionCourrier
    {
        [Key]
        public int Id { get; set; }

        public int CourrierId { get; set; }
        public virtual Courrier? Courrier { get; set; }

        public int? ServiceId { get; set; }
        public virtual Service? Service { get; set; }

        public string? UserDiffId { get; set; }
        public virtual User? UserDiff { get; set; }

        public string? Commentaire { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}