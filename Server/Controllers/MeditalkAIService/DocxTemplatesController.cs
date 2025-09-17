using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers
{
    [Route("odata/VanigamAccountingService/DocxTemplates")]
    public class DocxTemplatesController(
        VanigamAccountingDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,     
        DocxTemplateService service)
        : BaseODataServiceController<DocxTemplate, DocxTemplateService>(context, userManager, roleManager,
             service, $"{nameof(DocxTemplate.FileCategory)}");
}

