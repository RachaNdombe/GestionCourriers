namespace JconsultGC.Models;

public class ActiveSession
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public bool IsCurrentSession { get; set; }
    
    // Navigation properties
    public virtual User? User { get; set; }
}