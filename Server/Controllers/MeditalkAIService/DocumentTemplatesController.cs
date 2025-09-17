using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Controllers;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers;

[Route("odata/VanigamAccountingService/DocumentTemplates")]
public class DocumentTemplatesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    DocumentTemplateService service)
    : BaseODataServiceController<DocumentTemplate, DocumentTemplateService>(context, userManager, roleManager,
         service, null);
