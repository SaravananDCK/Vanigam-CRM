using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Opportunities)}")]
    public class OpportunitiesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    OpportunityService service,
    SummaryService<Opportunity, OpportunityStage> summaryService,
    ILogger<OpportunitiesController> logger)
    : BaseODataServiceController<Opportunity, OpportunityService>(context, userManager, roleManager,
        service, null)
    {
        /// <summary>
        /// Gets status summary with counts for each OpportunityStage in a single request
        /// </summary>
        /// <param name="request">Summary request with optional filters</param>
        /// <returns>Status summary response with counts</returns>
        [HttpPost("status-summary")]
        [Route("status-summary")]
        public async Task<ActionResult<StatusSummaryResponse<OpportunityStage>>> GetStatusSummary(
            [FromBody] StatusSummaryRequest request)
        {
            try
            {
                logger.LogInformation("Getting opportunity status summary with search filter: {SearchFilter}",
                    request.SearchFilter);

                var result = await summaryService.GetStatusSummaryAsync(
                    Context.Opportunities,
                    opportunity => opportunity.Stage,
                    request.SearchFilter,
                    request.AdditionalFilter);

                logger.LogInformation("Opportunity status summary completed: Total={TotalCount}, Stages={StageCount}",
                    result.TotalCount, result.StatusCounts.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting opportunity status summary");
                return BadRequest(new { Error = "Failed to retrieve status summary" });
            }
        }
    }
}