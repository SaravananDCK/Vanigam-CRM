using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers;

[Route("odata/VanigamAccountingService/FileCategories")]
public class FileCategoriesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    FileCategoryService service)
    : BaseODataServiceController<FileCategory, FileCategoryService>(context, userManager, roleManager,
         service, null);
