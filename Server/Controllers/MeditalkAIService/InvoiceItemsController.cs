using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.InvoiceItems)}")]
    public class InvoiceItemsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    InvoiceItemService service)
    : BaseODataServiceController<InvoiceItem, InvoiceItemService>(context, userManager, roleManager,
        service, null);
}
