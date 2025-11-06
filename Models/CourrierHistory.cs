using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models
{
    public class CourrierHistory
    {
        [Key]
        public int Id { get; set; }

        public int CourrierId { get; set; }
        public virtual Courrier? Courrier { get; set; }

        public string? UserId { get; set; }
        public virtual User? User { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? Note { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}