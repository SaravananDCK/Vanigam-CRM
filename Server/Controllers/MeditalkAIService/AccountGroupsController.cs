using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService;

[Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.AccountGroups)}")]
public class AccountGroupsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    AccountGroupService service)
    : BaseODataServiceController<AccountGroup, AccountGroupService>(context, userManager, roleManager, service, null);
