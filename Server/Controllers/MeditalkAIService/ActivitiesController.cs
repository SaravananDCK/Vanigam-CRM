using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Activities)}")]
    public class ActivitiesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ActivityService service,
    SummaryService<Activity, ActivityStatus> summaryService,
    ILogger<ActivitiesController> logger)
    : BaseODataServiceController<Activity, ActivityService>(context, userManager, roleManager,
        service, null)
    {
        /// <summary>
        /// Gets status summary with counts for each ActivityStatus in a single request
        /// </summary>
        /// <param name="request">Summary request with optional filters</param>
        /// <returns>Status summary response with counts</returns>
        [HttpPost("status-summary")]
        [Route("status-summary")]
        public async Task<ActionResult<StatusSummaryResponse<ActivityStatus>>> GetStatusSummary(
            [FromBody] StatusSummaryRequest request)
        {
            try
            {
                logger.LogInformation("Getting activity status summary with search filter: {SearchFilter}",
                    request.SearchFilter);

                var result = await summaryService.GetStatusSummaryAsync(
                    Context.Activities,
                    activity => activity.Status,
                    request.SearchFilter,
                    request.AdditionalFilter);

                logger.LogInformation("Activity status summary completed: Total={TotalCount}, Statuses={StatusCount}",
                    result.TotalCount, result.StatusCounts.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting activity status summary");
                return BadRequest(new { Error = "Failed to retrieve status summary" });
            }
        }
    }
}