namespace JconsultGC.Models;

public class TwoFactorManagementViewModel
{
    public bool IsEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public string? AuthenticatorKey { get; set; }
}