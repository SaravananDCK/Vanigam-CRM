using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class ProductCategoryController(ApplicationDbContext context, ILogger<ProductCategoryController> logger)
    : BaseApiController<ProductCategory, long>(context, logger)
{
    protected override DbSet<ProductCategory> DbSet => Context.ProductCategory;
    protected override Expression<Func<ProductCategory, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "ProductCategory";
}
