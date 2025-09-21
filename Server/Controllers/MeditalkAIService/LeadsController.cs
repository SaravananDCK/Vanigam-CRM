using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Leads)}")]
    public class LeadsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    LeadService service,
    SummaryService<Lead, LeadStatus> summaryService,
    ILogger<LeadsController> logger)
    : BaseODataServiceController<Lead, LeadService>(context, userManager, roleManager,
        service, null)
    {
        /// <summary>
        /// Gets status summary with counts for each LeadStatus in a single request
        /// </summary>
        /// <param name="request">Summary request with optional filters</param>
        /// <returns>Status summary response with counts</returns>
        [HttpPost("status-summary")]
        [Route("status-summary")]
        public async Task<ActionResult<StatusSummaryResponse<LeadStatus>>> GetStatusSummary(
            [FromBody] StatusSummaryRequest request)
        {
            try
            {
                logger.LogInformation("Getting lead status summary with search filter: {SearchFilter}",
                    request.SearchFilter);

                var result = await summaryService.GetStatusSummaryAsync(
                    Context.Leads,
                    lead => lead.Status,
                    request.SearchFilter,
                    request.AdditionalFilter);

                logger.LogInformation("Lead status summary completed: Total={TotalCount}, Statuses={StatusCount}",
                    result.TotalCount, result.StatusCounts.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting lead status summary");
                return BadRequest(new { Error = "Failed to retrieve status summary" });
            }
        }

        /// <summary>
        /// Converts a Lead to an Opportunity
        /// </summary>
        /// <param name="request">Conversion request with lead details</param>
        /// <returns>The created Opportunity</returns>
        [HttpPost("convert-to-opportunity")]
        [Route("convert-to-opportunity")]
        public async Task<ActionResult<Opportunity>> ConvertLeadToOpportunity([FromBody] ConvertLeadToOpportunityRequest request)
        {
            try
            {
                logger.LogInformation("Converting lead {LeadId} to opportunity", request.LeadId);

                var opportunity = await service.ConvertLeadToOpportunityAsync(
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

        /// <summary>
        /// Converts an Opportunity to a Customer
        /// </summary>
        /// <param name="request">Conversion request with opportunity details</param>
        /// <returns>The created Customer</returns>
        [HttpPost("convert-to-customer")]
        [Route("convert-to-customer")]
        public async Task<ActionResult<Customer>> ConvertOpportunityToCustomer([FromBody] ConvertOpportunityToCustomerRequest request)
        {
            try
            {
                logger.LogInformation("Converting opportunity {OpportunityId} to customer", request.OpportunityId);

                var customer = await service.ConvertOpportunityToCustomerAsync(request.OpportunityId);

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
}