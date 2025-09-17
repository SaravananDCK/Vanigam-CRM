using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Vanigam.CRM.Objects.Helpers;
using Vanigam.CRM.Objects.Services;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Helpers;

public class EmailSender(
    IOptions<MailJetOptions> optionsAccessor,
    IOptions<VanigamAccountingOptions> vanigamAccountingOption,
    ICurrentUserService currentUserService,
    ILogger<EmailSender> logger)
    : IEmailSender<ApplicationUser>
{
    public IOptions<VanigamAccountingOptions> VanigamAccountingOption { get; } = vanigamAccountingOption;
    public ICurrentUserService CurrentUserService { get; } = currentUserService;
    private readonly ILogger _logger = logger;

    public MailJetOptions Options { get; } = optionsAccessor.Value;

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        if (VanigamAccountingOption.Value.Configuration != VanigamAccountingOptions.LiveConfiguration)
        {
            email = CurrentUserService.Email;
        }

        if (string.IsNullOrEmpty(Options.ApiKey) || string.IsNullOrEmpty(Options.SecretKey))
        {
            throw new Exception("Mailjet ApiKey or SecretKey is not configured.");
        }

        if (!int.TryParse(Options.InvitationTemplateId, out var templateId))
        {
            throw new Exception($"Invalid Mailjet InvitationTemplateId: '{Options.InvitationTemplateId}'. Must be an integer.");
        }

        var client = new MailjetClient(Options.ApiKey, Options.SecretKey);

        // construct your email with builder
        var emailBuild = new TransactionalEmailBuilder()
            .WithFrom(new SendContact("saravanan@tekspear.com", "Vanigam Accounting"))
            .WithSubject(Options.InvitationSubject ?? "Confirm your account")
            .WithTo(new SendContact(email, user.Name))
            .WithTemplateId(templateId)
            .WithTemplateLanguage(true)
            .WithVariable("recipientName", user.Name)
            .WithVariable("linkCode", confirmationLink)
            .Build();
        var response = await client.SendTransactionalEmailAsync(emailBuild);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        if (VanigamAccountingOption.Value.Configuration != VanigamAccountingOptions.LiveConfiguration)
        {
            email = CurrentUserService.Email;
        }

        if (string.IsNullOrEmpty(Options.ApiKey) || string.IsNullOrEmpty(Options.SecretKey))
        {
            throw new Exception("Mailjet ApiKey or SecretKey is not configured.");
        }

        if (!int.TryParse(Options.ResetPasswordTemplateId, out var templateId))
        {
            throw new Exception($"Invalid Mailjet ResetPasswordTemplateId: '{Options.ResetPasswordTemplateId}'. Must be an integer.");
        }

        var client = new MailjetClient(Options.ApiKey, Options.SecretKey){};

        MailjetRequest request = new MailjetRequest
        {
            Resource = Send.Resource
        };

        // construct your email with builder
        var emailBuild = new TransactionalEmailBuilder()
            .WithFrom(new SendContact("saravanan@tekspear.com", "Vanigam Accounting"))
            .WithSubject(Options.InvitationSubject ?? "Confirm your account")
            .WithTo(new SendContact(email, user.Name))
            .WithTemplateId(templateId)
            .WithTemplateLanguage(true)
            .WithVariable("recipientName", user.Name)
            .WithVariable("linkCode", resetLink)
            .Build();
        var response = await client.SendTransactionalEmailAsync(emailBuild);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {

    }
}
