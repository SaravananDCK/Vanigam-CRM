using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService;

[Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.ServiceItems)}")]
public class ServiceItemsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ServiceItemService service)
    : BaseODataServiceController<ServiceItem, ServiceItemService>(context, userManager, roleManager,
        service, null);