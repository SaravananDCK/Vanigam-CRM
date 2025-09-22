using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services;

public class WebhookLeadService(
    VanigamAccountingDbContext context,
    ILogger<WebhookLeadService> logger,
    ICurrentUserService currentUserService,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<Lead> CreateLeadFromWebhookAsync(WebhookLeadRequest request)
    {
        try
        {
            var tenantId = currentUserService?.TenantId ?? throw new UnauthorizedAccessException("No tenant context");

            var lead = new Lead
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Source = request.Source,
                Status = LeadStatus.New,
                Description = request.Message,
                TenantId = tenantId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            if (request.AdditionalData != null)
            {
                lead.Comments = JsonSerializer.Serialize(request.AdditionalData);
            }

            context.Leads.Add(lead);
            await context.SaveChangesAsync();

            logger.LogInformation("Created lead {LeadId} from {Platform} webhook for {Name}",
                lead.Oid, request.Platform, request.Name);

            var activity = new Activity
            {
                Type = ActivityType.LeadConversion,
                Subject = $"Lead created from {request.Platform}",
                Description = $"Lead automatically created from {request.Platform} webhook submission",
                Status = ActivityStatus.Completed,
                ActivityDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                LeadId = lead.Oid,
                TenantId = tenantId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            context.Activities.Add(activity);
            await context.SaveChangesAsync();

            return lead;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lead from webhook: {Platform}, {Name}",
                request.Platform, request.Name);
            throw;
        }
    }

    public async Task<FacebookLeadDetails> GetFacebookLeadDetailsAsync(string leadId, string accessToken)
    {
        try
        {
            var url = $"https://graph.facebook.com/v18.0/{leadId}?fields=id,created_time,field_data&access_token={accessToken}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var leadDetails = JsonSerializer.Deserialize<FacebookLeadDetails>(json);

            return leadDetails ?? new FacebookLeadDetails();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching Facebook lead details for {LeadId}", leadId);
            throw;
        }
    }

    public WebhookLeadRequest MapFacebookWebhookToLead(FacebookLeadDetails facebookLead)
    {
        var request = new WebhookLeadRequest
        {
            Platform = "Facebook",
            Source = "Facebook Lead Ads"
        };

        foreach (var field in facebookLead.FieldData)
        {
            var value = field.Values.FirstOrDefault() ?? string.Empty;

            switch (field.Name.ToLowerInvariant())
            {
                case "full_name":
                case "name":
                case "first_name":
                    request.Name = string.IsNullOrEmpty(request.Name) ? value : $"{request.Name} {value}";
                    break;
                case "email":
                    request.Email = value;
                    break;
                case "phone_number":
                case "phone":
                    request.Phone = value;
                    break;
                case "message":
                case "comments":
                case "description":
                    request.Message = value;
                    break;
                default:
                    request.AdditionalData ??= new Dictionary<string, object>();
                    request.AdditionalData[field.Name] = value;
                    break;
            }
        }

        return request;
    }

    public WebhookLeadRequest MapWhatsAppWebhookToLead(WhatsAppWebhookDto whatsAppWebhook)
    {
        var entry = whatsAppWebhook.Entry.FirstOrDefault();
        var change = entry?.Changes.FirstOrDefault();
        var contact = change?.Value.Contacts.FirstOrDefault();
        var message = change?.Value.Messages.FirstOrDefault();

        return new WebhookLeadRequest
        {
            Platform = "WhatsApp",
            Source = "WhatsApp Business",
            Name = contact?.Profile.Name ?? "WhatsApp Contact",
            Phone = contact?.WaId,
            Message = message?.Text?.Body,
            AdditionalData = new Dictionary<string, object>
            {
                ["phone_number_id"] = change?.Value.Metadata.PhoneNumberId ?? string.Empty,
                ["message_id"] = message?.Id ?? string.Empty,
                ["timestamp"] = message?.Timestamp ?? string.Empty
            }
        };
    }

    public WebhookLeadRequest MapInstagramWebhookToLead(InstagramWebhookDto instagramWebhook)
    {
        var entry = instagramWebhook.Entry.FirstOrDefault();
        var messaging = entry?.Messaging.FirstOrDefault();

        return new WebhookLeadRequest
        {
            Platform = "Instagram",
            Source = "Instagram Direct Message",
            Name = $"Instagram User {messaging?.Sender.Id}",
            Message = messaging?.Message?.Text,
            AdditionalData = new Dictionary<string, object>
            {
                ["sender_id"] = messaging?.Sender.Id ?? string.Empty,
                ["recipient_id"] = messaging?.Recipient.Id ?? string.Empty,
                ["message_id"] = messaging?.Message?.Mid ?? string.Empty,
                ["timestamp"] = messaging?.Timestamp.ToString() ?? string.Empty
            }
        };
    }

    public WebhookLeadRequest MapGenericWebhookToLead(GenericSocialMediaWebhookDto genericWebhook)
    {
        return new WebhookLeadRequest
        {
            Platform = genericWebhook.Platform,
            Source = $"{genericWebhook.Platform} Integration",
            Name = genericWebhook.ContactName,
            Email = genericWebhook.ContactEmail,
            Phone = genericWebhook.ContactPhone,
            Message = genericWebhook.Message,
            AdditionalData = new Dictionary<string, object>
            {
                ["source_id"] = genericWebhook.SourceId ?? string.Empty,
                ["campaign_info"] = genericWebhook.CampaignInfo ?? new Dictionary<string, object>(),
                ["custom_fields"] = genericWebhook.CustomFields ?? new Dictionary<string, object>()
            }
        };
    }

    public bool ValidateWebhookSignature(string payload, string signature, string secret)
    {
        try
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var computedSignature = "sha256=" + Convert.ToHexString(computedHash).ToLowerInvariant();

            return string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating webhook signature");
            return false;
        }
    }
}