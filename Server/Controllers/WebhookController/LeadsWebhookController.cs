using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.WebhookController;

[ApiController]
[Route("api/webhooks/leads")]
[AllowAnonymous]
public class LeadsWebhookController(
    WebhookLeadService webhookLeadService,
    ILogger<LeadsWebhookController> logger,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("facebook/verify")]
    public IActionResult VerifyFacebookWebhook([FromQuery] string? hub_mode, [FromQuery] string? hub_challenge, [FromQuery] string? hub_verify_token)
    {
        var verifyToken = configuration["Webhook:Facebook:VerifyToken"];

        if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
        {
            logger.LogInformation("Facebook webhook verified successfully");
            return Ok(hub_challenge);
        }

        logger.LogWarning("Facebook webhook verification failed. Mode: {Mode}, Token: {Token}", hub_mode, hub_verify_token);
        return BadRequest("Verification failed");
    }

    [HttpPost("facebook")]
    public async Task<IActionResult> HandleFacebookWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();

            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var secret = configuration["Webhook:Facebook:AppSecret"];

            if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(signature))
            {
                if (!webhookLeadService.ValidateWebhookSignature(payload, signature, secret))
                {
                    logger.LogWarning("Facebook webhook signature validation failed");
                    return Unauthorized("Invalid signature");
                }
            }

            var webhookData = JsonSerializer.Deserialize<FacebookWebhookDto>(payload);
            if (webhookData?.Entry == null || !webhookData.Entry.Any())
            {
                return Ok(new WebhookResponse { Success = true, Message = "No entries to process" });
            }

            var accessToken = configuration["Webhook:Facebook:AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                logger.LogError("Facebook access token not configured");
                return BadRequest(new WebhookResponse { Success = false, Message = "Access token not configured" });
            }

            foreach (var entry in webhookData.Entry)
            {
                foreach (var change in entry.Changes)
                {
                    if (change.Field == "leadgen")
                    {
                        try
                        {
                            var leadDetails = await webhookLeadService.GetFacebookLeadDetailsAsync(
                                change.Value.LeadgenId, accessToken);

                            var leadRequest = webhookLeadService.MapFacebookWebhookToLead(leadDetails);
                            leadRequest.AdditionalData ??= new Dictionary<string, object>();
                            leadRequest.AdditionalData["form_id"] = change.Value.FormId;
                            leadRequest.AdditionalData["page_id"] = change.Value.PageId;
                            leadRequest.AdditionalData["ad_id"] = change.Value.AdId ?? string.Empty;
                            leadRequest.AdditionalData["campaign_id"] = change.Value.CampaignId ?? string.Empty;

                            var lead = await webhookLeadService.CreateLeadFromWebhookAsync(leadRequest);
                            logger.LogInformation("Created lead {LeadId} from Facebook webhook", lead.Oid);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing Facebook lead {LeadId}", change.Value.LeadgenId);
                        }
                    }
                }
            }

            return Ok(new WebhookResponse { Success = true, Message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Facebook webhook");
            return BadRequest(new WebhookResponse { Success = false, Message = "Error processing webhook" });
        }
    }

    [HttpGet("whatsapp/verify")]
    public IActionResult VerifyWhatsAppWebhook([FromQuery] string? hub_mode, [FromQuery] string? hub_challenge, [FromQuery] string? hub_verify_token)
    {
        var verifyToken = configuration["Webhook:WhatsApp:VerifyToken"];

        if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
        {
            logger.LogInformation("WhatsApp webhook verified successfully");
            return Ok(hub_challenge);
        }

        logger.LogWarning("WhatsApp webhook verification failed. Mode: {Mode}, Token: {Token}", hub_mode, hub_verify_token);
        return BadRequest("Verification failed");
    }

    [HttpPost("whatsapp")]
    public async Task<IActionResult> HandleWhatsAppWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();

            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var secret = configuration["Webhook:WhatsApp:AppSecret"];

            if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(signature))
            {
                if (!webhookLeadService.ValidateWebhookSignature(payload, signature, secret))
                {
                    logger.LogWarning("WhatsApp webhook signature validation failed");
                    return Unauthorized("Invalid signature");
                }
            }

            var webhookData = JsonSerializer.Deserialize<WhatsAppWebhookDto>(payload);
            if (webhookData?.Entry == null || !webhookData.Entry.Any())
            {
                return Ok(new WebhookResponse { Success = true, Message = "No entries to process" });
            }

            foreach (var entry in webhookData.Entry)
            {
                foreach (var change in entry.Changes)
                {
                    if (change.Field == "messages" && change.Value.Messages.Any())
                    {
                        try
                        {
                            var leadRequest = webhookLeadService.MapWhatsAppWebhookToLead(webhookData);
                            var lead = await webhookLeadService.CreateLeadFromWebhookAsync(leadRequest);
                            logger.LogInformation("Created lead {LeadId} from WhatsApp webhook", lead.Oid);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing WhatsApp message for entry {EntryId}", entry.Id);
                        }
                    }
                }
            }

            return Ok(new WebhookResponse { Success = true, Message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing WhatsApp webhook");
            return BadRequest(new WebhookResponse { Success = false, Message = "Error processing webhook" });
        }
    }

    [HttpGet("instagram/verify")]
    public IActionResult VerifyInstagramWebhook([FromQuery] string? hub_mode, [FromQuery] string? hub_challenge, [FromQuery] string? hub_verify_token)
    {
        var verifyToken = configuration["Webhook:Instagram:VerifyToken"];

        if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
        {
            logger.LogInformation("Instagram webhook verified successfully");
            return Ok(hub_challenge);
        }

        logger.LogWarning("Instagram webhook verification failed. Mode: {Mode}, Token: {Token}", hub_mode, hub_verify_token);
        return BadRequest("Verification failed");
    }

    [HttpPost("instagram")]
    public async Task<IActionResult> HandleInstagramWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();

            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var secret = configuration["Webhook:Instagram:AppSecret"];

            if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(signature))
            {
                if (!webhookLeadService.ValidateWebhookSignature(payload, signature, secret))
                {
                    logger.LogWarning("Instagram webhook signature validation failed");
                    return Unauthorized("Invalid signature");
                }
            }

            var webhookData = JsonSerializer.Deserialize<InstagramWebhookDto>(payload);
            if (webhookData?.Entry == null || !webhookData.Entry.Any())
            {
                return Ok(new WebhookResponse { Success = true, Message = "No entries to process" });
            }

            foreach (var entry in webhookData.Entry)
            {
                foreach (var messaging in entry.Messaging)
                {
                    if (messaging.Message != null)
                    {
                        try
                        {
                            var leadRequest = webhookLeadService.MapInstagramWebhookToLead(webhookData);
                            var lead = await webhookLeadService.CreateLeadFromWebhookAsync(leadRequest);
                            logger.LogInformation("Created lead {LeadId} from Instagram webhook", lead.Oid);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing Instagram message for sender {SenderId}", messaging.Sender.Id);
                        }
                    }
                }
            }

            return Ok(new WebhookResponse { Success = true, Message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Instagram webhook");
            return BadRequest(new WebhookResponse { Success = false, Message = "Error processing webhook" });
        }
    }

    [HttpPost("generic")]
    public async Task<IActionResult> HandleGenericWebhook([FromBody] GenericSocialMediaWebhookDto webhookData)
    {
        try
        {
            if (string.IsNullOrEmpty(webhookData.ContactName))
            {
                return BadRequest(new WebhookResponse { Success = false, Message = "Contact name is required" });
            }

            var leadRequest = webhookLeadService.MapGenericWebhookToLead(webhookData);
            var lead = await webhookLeadService.CreateLeadFromWebhookAsync(leadRequest);

            logger.LogInformation("Created lead {LeadId} from {Platform} generic webhook", lead.Oid, webhookData.Platform);

            return Ok(new WebhookResponse
            {
                Success = true,
                Message = "Lead created successfully",
                LeadId = lead.Oid
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing {Platform} generic webhook", webhookData.Platform);
            return BadRequest(new WebhookResponse { Success = false, Message = "Error processing webhook" });
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}