using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Payments)}")]
    public class PaymentsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    PaymentService service)
    : BaseODataServiceController<Payment, PaymentService>(context, userManager, roleManager,
        service, null)
    {
        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Payment>> BulkSavePaymentWithAllocations([FromBody] PaymentBulkSaveDTO paymentData)
        {
            try
            {
                // Use service method which ensures proper ledger posting
                var savedPayment = await service.BulkSavePaymentWithAllocations(paymentData, CurrentUserId.ToString());
                return Ok(savedPayment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error saving payment: {ex.Message}" });
            }
        }
    }
}
