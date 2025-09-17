using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers
{
    [Route("odata/VanigamAccountingService/DocxMacroTemplates")]
    public class DocxMacroTemplatesController(
        VanigamAccountingDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        DocxMacroTemplateService service)
        : BaseODataServiceController<DocxMacroTemplate, DocxMacroTemplateService>(context, userManager, roleManager,
             service, $"{nameof(DocxTemplate.FileCategory)}");
}

