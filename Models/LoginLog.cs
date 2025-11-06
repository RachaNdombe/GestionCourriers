namespace JconsultGC.Models;

public class LoginLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? SessionId { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
}