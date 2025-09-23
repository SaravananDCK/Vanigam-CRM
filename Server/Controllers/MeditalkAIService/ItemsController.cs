using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService;

[Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Items)}")]
public class ItemsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ItemService service)
    : BaseODataServiceController<Item, ItemService>(context, userManager, roleManager,
        service, null);