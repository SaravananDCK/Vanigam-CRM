using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ProductService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Product>> logger)
    : BaseService<Product>(context, logger)
{
    public override DbSet<Product> GetDbSet()
    {
        return Context.Products;
    }
}