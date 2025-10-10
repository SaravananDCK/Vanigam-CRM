using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService;

[Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Products)}")]
public class ProductsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ProductService service)
    : BaseODataServiceController<Product, ProductService>(context, userManager, roleManager,
        service, null);