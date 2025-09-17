using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Server.Controllers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.Server.Controllers.VanigamConfiguration;

[Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.ReportTemplates)}")]
public class ReportTemplatesController(VanigamAccountingDbContext context, UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager, ReportTemplateService service)
    : BaseODataServiceController<ReportTemplate, ReportTemplateService>(context, userManager, roleManager, service, string.Empty)
{

}

