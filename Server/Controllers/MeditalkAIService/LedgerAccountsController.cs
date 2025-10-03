using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.LedgerAccounts)}")]
    public class LedgerAccountsController(
        VanigamAccountingDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        LedgerAccountService service)
        : BaseODataServiceController<LedgerAccount, LedgerAccountService>(context, userManager, roleManager,
            service, null);
}
