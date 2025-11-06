namespace JconsultGC.Models;

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? ServiceNom { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public TwoFactorManagementViewModel TwoFactorModel { get; set; } = new();
    public LoginLogsViewModel LoginLogsModel { get; set; } = new();
    public ActiveSessionsViewModel ActiveSessionsModel { get; set; } = new();
}

public class LoginLogsViewModel
{
    public List<LoginLogItemViewModel> Logs { get; set; } = new();
}

public class LoginLogItemViewModel
{
    public int Id { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public bool IsTwoFactorUsed { get; set; }
    public string? SessionDuration { get; set; }
}

public class ActiveSessionsViewModel
{
    public List<ActiveSessionItemViewModel> Sessions { get; set; } = new();
    public string? CurrentSessionId { get; set; }
}

public class ActiveSessionItemViewModel
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? MachineName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public string? UserAgent { get; set; }
    public bool IsCurrentSession { get; set; }
    public string? SessionDuration { get; set; }
}