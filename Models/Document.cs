using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JconsultGC.Models;

public class Document
{
    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    public string? FileName { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public string? FilePath { get; set; }

    public int? CourrierId { get; set; }
    public virtual Courrier? Courrier { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
