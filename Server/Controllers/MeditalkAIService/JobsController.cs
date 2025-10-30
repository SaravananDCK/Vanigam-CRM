using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Jobs)}")]
    public class JobsController(
        VanigamAccountingDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        JobService service,
        SummaryService<Job, JobStatus> summaryService,
        ILogger<JobsController> logger)
        : BaseODataServiceController<Job, JobService>(context, userManager, roleManager,
            service, null)
    {
        [HttpPost("status-summary")]
        [Route("status-summary")]
        public async Task<ActionResult<StatusSummaryResponse<JobStatus>>> GetStatusSummary(
            [FromBody] StatusSummaryRequest request)
        {
            try
            {
                logger.LogInformation("Getting Job status summary with search filter: {SearchFilter}",
                    request.SearchFilter);

                var result = await summaryService.GetStatusSummaryAsync(
                    Context.Jobs,
                    job => job.Status,
                    request.SearchFilter,
                    request.AdditionalFilter);

                logger.LogInformation("Job status summary completed: Total={TotalCount}, Statuses={StatusCount}",
                    result.TotalCount, result.StatusCounts.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting Job status summary");
                return BadRequest(new { Error = "Failed to retrieve status summary" });
            }
        }

        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Job>> BulkSaveJobWithMaterials([FromBody] JobBulkSaveDTO jobData)
        {
            try
            {
                var savedJob = await service.BulkSaveJobWithMaterials(jobData);
                return Ok(savedJob);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error saving job: {ex.Message}" });
            }
        }

        [HttpGet("/api/job/{jobId}/materials-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<MaterialUsageDTO>>> GetMaterialsForEditing(Guid jobId)
        {
            try
            {
                var materials = await context.MaterialUsages
                    .Where(m => m.VoucherId == jobId)
                    .Include(m => m.Item)
                    .Include(m => m.TaxCode)
                    .Select(m => new MaterialUsageDTO
                    {
                        Oid = m.Oid,
                        InventoryItemId = m.ItemId,
                        Quantity = m.Quantity,
                        UnitPrice = m.UnitPrice,
                        DiscountAmount = m.DiscountAmount,
                        TaxAmount = m.TaxAmount,
                        TaxCodeId = m.TaxCodeId,
                        CGSTRate = m.TaxCode != null ? m.TaxCode.CGSTRate : 0,
                        SGSTRate = m.TaxCode != null ? m.TaxCode.SGSTRate : 0,
                        IGSTRate = m.TaxCode != null ? m.TaxCode.IGSTRate : 0,
                        CessRate = m.TaxCode != null ? m.TaxCode.CessRate : 0,
                        InventoryItemName = m.Item != null ? m.Item.Name : null,
                        ChargedAmount = m.ChargedAmount,
                        WaivedAmount = m.WaivedAmount
                    })
                    .ToListAsync();

                return Ok(materials);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading materials: {ex.Message}");
            }
        }
    }
}