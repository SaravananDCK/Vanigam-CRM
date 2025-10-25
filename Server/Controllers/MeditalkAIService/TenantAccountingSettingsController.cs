using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService;

[Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.TenantAccountingSettings)}")]
public class TenantAccountingSettingsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    TenantAccountingSettingsService service)
    : BaseODataServiceController<TenantAccountingSettings, TenantAccountingSettingsService>(context, userManager, roleManager,
        service, null);
