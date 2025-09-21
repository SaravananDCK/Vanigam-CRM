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
    }
}