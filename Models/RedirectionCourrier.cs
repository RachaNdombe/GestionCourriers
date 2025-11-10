using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models
{
    public class RedirectionCourrier
    {
        [Key]
        public int Id { get; set; }

        public int CourrierId { get; set; }
        public virtual Courrier? Courrier { get; set; }

        public string? UserRedirigerId { get; set; }
        public virtual User? UserRediriger { get; set; }

        public TypeTransmission Transmission { get; set; }
        public StatutTraiter Traiter { get; set; }

        public string? Commentaire { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}