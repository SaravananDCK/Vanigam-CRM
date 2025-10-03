using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.NumberSeries)}")]
    public class NumberSeriesController(
        VanigamAccountingDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        NumberSeriesService service)
        : BaseODataServiceController<NumberSeries, NumberSeriesService>(context, userManager, roleManager,
            service, null)
    {
        /// <summary>
        /// Generates the next number for the specified entity type
        /// POST: odata/VanigamAccountingService/NumberSeries/GenerateNextNumber
        /// </summary>
        [HttpPost("GenerateNextNumber")]
        public async Task<ActionResult<string>> GenerateNextNumber([FromBody] GenerateNumberRequest request)
        {
            try
            {
                var nextNumber = await service.GenerateNextNumber(request.EntityType, Service.TenantId);
                return Ok(new { Number = nextNumber });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while generating the next number" });
            }
        }

        /// <summary>
        /// Previews the next number without incrementing
        /// GET: odata/VanigamAccountingService/NumberSeries/PreviewNextNumber?entityType=Quote
        /// </summary>
        [HttpGet("PreviewNextNumber")]
        public async Task<ActionResult<string>> PreviewNextNumber([FromQuery] string entityType)
        {
            try
            {
                var preview = await service.PreviewNextNumber(entityType, Service.TenantId);
                if (preview == null)
                    return NotFound(new { Error = $"No active number series found for entity type '{entityType}'" });

                return Ok(new { Preview = preview });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while previewing the next number" });
            }
        }

        /// <summary>
        /// Resets the number series to its starting number
        /// POST: odata/VanigamAccountingService/NumberSeries/ResetNumberSeries
        /// </summary>
        [HttpPost("ResetNumberSeries")]
        public async Task<ActionResult> ResetNumberSeries([FromBody] GenerateNumberRequest request)
        {
            try
            {
                await service.ResetNumberSeries(request.EntityType, Service.TenantId);
                return Ok(new { Message = $"Number series for '{request.EntityType}' has been reset" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while resetting the number series" });
            }
        }
    }

    public class GenerateNumberRequest
    {
        public string EntityType { get; set; } = string.Empty;
    }
}
