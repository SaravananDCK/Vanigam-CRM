using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
    : BaseApiController<Product, long>(context, logger)
{
    protected override DbSet<Product> DbSet => Context.Product;
    protected override Expression<Func<Product, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Product";

    protected override IQueryable<Product> GetQueryWithIncludes()
    {
        return Context.Product.Include(x => x.ProductCategory);
    }

    protected override async Task OnBeforeCreateAsync(Product product)
    {
        var productCategory = product.ProductCategory;
        product.ProductCategory = null;

        if (productCategory != null)
        {
            var newValues = await Context.ProductCategory
                .Where(x => productCategory.Select(y => y.Id).Contains(x.Id))
                .ToListAsync();
            product.ProductCategory = [..newValues];
        }
    }

    protected override async Task OnBeforeUpdateAsync(Product existing, Product update)
    {
        await Context.Entry(existing).Collection(x => x.ProductCategory).LoadAsync();

        if (update.ProductCategory != null)
        {
            var updateValues = update.ProductCategory.Select(x => x.Id);
            existing.ProductCategory ??= [];
            existing.ProductCategory.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !existing.ProductCategory.Select(y => y.Id).Contains(x));
            var newValues = await Context.ProductCategory.Where(x => addValues.Contains(x.Id)).ToListAsync();
            existing.ProductCategory.AddRange(newValues);
        }
    }
}
