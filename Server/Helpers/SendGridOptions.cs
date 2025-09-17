namespace Vanigam.CRM.Server.Helpers;

public class SendGridOptions
{
    public string? Key { get; set; }
    public string? InvitationTemplateId { get; set; }
    public string? InvitationSubject { get; set; }
    public string? ResetPasswordTemplateId { get; set; }
    public string? ResetPasswordSubject { get; set; }
}
public class MailJetOptions
{
    public string ApiKey { get; set; }
    public string SecretKey { get; set; }
    public string? InvitationTemplateId { get; set; }
    public string? InvitationSubject { get; set; }
    public string? ResetPasswordTemplateId { get; set; }
    public string? ResetPasswordSubject { get; set; }
}
