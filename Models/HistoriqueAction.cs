using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models
{
    public class HistoriqueAction
    {
        [Key]
        public int Id { get; set; }

        public int CourrierId { get; set; }
        public virtual Courrier? Courrier { get; set; }

        public string? ByUserId { get; set; }
        public virtual User? ByUser { get; set; }

        public string ActionLibelle { get; set; } = string.Empty;
        public string? Commentaire { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}