using Alexinea.FastMember;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Serilog;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers;

[Route("odata/VanigamAccountingService/FileDocuments")]
public class FileDocumentsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    FileDocumentService service)
    : BaseODataServiceController<FileDocument, FileDocumentService>(context, userManager, roleManager,
        service, null)
{
}
