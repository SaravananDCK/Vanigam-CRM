using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadConversionController(
    LeadConversionService conversionService,
    ILogger<LeadConversionController> logger) : ControllerBase
{
    [HttpPost("lead-to-opportunity")]
    public async Task<ActionResult<Opportunity>> ConvertLeadToOpportunity([FromBody] ConvertLeadToOpportunityRequest request)
    {
        try
        {
            logger.LogInformation("Converting lead {LeadId} to opportunity", request.LeadId);

            var opportunity = await conversionService.ConvertLeadToOpportunityAsync(
                request.LeadId,
                request.OpportunityTitle,
                request.EstimatedValue,
                request.ExpectedCloseDate);

            return Ok(opportunity);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while converting lead {LeadId}", request.LeadId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error converting lead {LeadId} to opportunity", request.LeadId);
            return StatusCode(500, new { error = "An error occurred while converting the lead to opportunity" });
        }
    }

    [HttpPost("opportunity-to-customer")]
    public async Task<ActionResult<Customer>> ConvertOpportunityToCustomer([FromBody] ConvertOpportunityToCustomerRequest request)
    {
        try
        {
            logger.LogInformation("Converting opportunity {OpportunityId} to customer", request.OpportunityId);

            var customer = await conversionService.ConvertOpportunityToCustomerAsync(request.OpportunityId);

            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while converting opportunity {OpportunityId}", request.OpportunityId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error converting opportunity {OpportunityId} to customer", request.OpportunityId);
            return StatusCode(500, new { error = "An error occurred while converting the opportunity to customer" });
        }
    }
}

public class ConvertLeadToOpportunityRequest
{
    public Guid LeadId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
}

public class ConvertOpportunityToCustomerRequest
{
    public Guid OpportunityId { get; set; }
}