using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.PurchaseOrderItems)}")]
    public class PurchaseOrderItemsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    PurchaseOrderItemService service)
    : BaseODataServiceController<PurchaseOrderItem, PurchaseOrderItemService>(context, userManager, roleManager,
        service, null);
}
