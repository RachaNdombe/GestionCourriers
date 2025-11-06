using System.ComponentModel.DataAnnotations;

namespace JconsultGC.Models;

public class Email
{
    [Key]
    public int Id { get; set; }

    [EmailAddress]
    public string? To { get; set; }

    [EmailAddress]
    public string? From { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public DateTime SentAt { get; set; }

    public bool IsSent { get; set; }
}
