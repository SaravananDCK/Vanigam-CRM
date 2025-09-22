using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.DTOs
{
    public class WebhookLeadRequest
    {
        public string Platform { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class FacebookWebhookDto
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<FacebookEntry> Entry { get; set; } = new();
    }

    public class FacebookEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("changes")]
        public List<FacebookChange> Changes { get; set; } = new();
    }

    public class FacebookChange
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public FacebookValue Value { get; set; } = new();
    }

    public class FacebookValue
    {
        [JsonPropertyName("form_id")]
        public string FormId { get; set; } = string.Empty;

        [JsonPropertyName("leadgen_id")]
        public string LeadgenId { get; set; } = string.Empty;

        [JsonPropertyName("created_time")]
        public long CreatedTime { get; set; }

        [JsonPropertyName("page_id")]
        public string PageId { get; set; } = string.Empty;

        [JsonPropertyName("adgroup_id")]
        public string? AdgroupId { get; set; }

        [JsonPropertyName("ad_id")]
        public string? AdId { get; set; }

        [JsonPropertyName("campaign_id")]
        public string? CampaignId { get; set; }
    }

    public class FacebookLeadDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("created_time")]
        public string CreatedTime { get; set; } = string.Empty;

        [JsonPropertyName("field_data")]
        public List<FacebookFieldData> FieldData { get; set; } = new();
    }

    public class FacebookFieldData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("values")]
        public List<string> Values { get; set; } = new();
    }

    public class WhatsAppWebhookDto
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<WhatsAppEntry> Entry { get; set; } = new();
    }

    public class WhatsAppEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public List<WhatsAppChange> Changes { get; set; } = new();
    }

    public class WhatsAppChange
    {
        [JsonPropertyName("value")]
        public WhatsAppValue Value { get; set; } = new();

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }

    public class WhatsAppValue
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public WhatsAppMetadata Metadata { get; set; } = new();

        [JsonPropertyName("contacts")]
        public List<WhatsAppContact> Contacts { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<WhatsAppMessage> Messages { get; set; } = new();
    }

    public class WhatsAppMetadata
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    public class WhatsAppContact
    {
        [JsonPropertyName("profile")]
        public WhatsAppProfile Profile { get; set; } = new();

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    public class WhatsAppProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class WhatsAppMessage
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public WhatsAppText? Text { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class WhatsAppText
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }

    public class InstagramWebhookDto
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<InstagramEntry> Entry { get; set; } = new();
    }

    public class InstagramEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("messaging")]
        public List<InstagramMessaging> Messaging { get; set; } = new();
    }

    public class InstagramMessaging
    {
        [JsonPropertyName("sender")]
        public InstagramSender Sender { get; set; } = new();

        [JsonPropertyName("recipient")]
        public InstagramRecipient Recipient { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("message")]
        public InstagramMessage? Message { get; set; }
    }

    public class InstagramSender
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class InstagramRecipient
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class InstagramMessage
    {
        [JsonPropertyName("mid")]
        public string Mid { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class GenericSocialMediaWebhookDto
    {
        [JsonPropertyName("platform")]
        public string Platform { get; set; } = string.Empty;

        [JsonPropertyName("contact_name")]
        public string ContactName { get; set; } = string.Empty;

        [JsonPropertyName("contact_email")]
        public string? ContactEmail { get; set; }

        [JsonPropertyName("contact_phone")]
        public string? ContactPhone { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("source_id")]
        public string? SourceId { get; set; }

        [JsonPropertyName("campaign_info")]
        public Dictionary<string, object>? CampaignInfo { get; set; }

        [JsonPropertyName("custom_fields")]
        public Dictionary<string, object>? CustomFields { get; set; }
    }

    public class WebhookVerificationRequest
    {
        [JsonPropertyName("hub.mode")]
        public string Mode { get; set; } = string.Empty;

        [JsonPropertyName("hub.challenge")]
        public string Challenge { get; set; } = string.Empty;

        [JsonPropertyName("hub.verify_token")]
        public string VerifyToken { get; set; } = string.Empty;
    }

    public class WebhookResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? LeadId { get; set; }
        public string? ErrorCode { get; set; }
    }
}