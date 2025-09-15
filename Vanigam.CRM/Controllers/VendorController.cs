using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class VendorController(ApplicationDbContext context, ILogger<VendorController> logger)
    : BaseApiController<Vendor, long>(context, logger)
{
    protected override DbSet<Vendor> DbSet => Context.Vendor;
    protected override Expression<Func<Vendor, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Vendor";

    protected override IQueryable<Vendor> GetQueryWithIncludes()
    {
        return Context.Vendor.Include(x => x.Address).Include(x => x.Product).Include(x => x.Service);
    }

    protected override async Task OnBeforeCreateAsync(Vendor vendor)
    {
        var product = vendor.Product;
        vendor.Product = null;

        var service = vendor.Service;
        vendor.Service = null;

        if (product != null)
        {
            var newValues = await Context.Product
                .Where(x => product.Select(y => y.Id).Contains(x.Id))
                .ToListAsync();
            vendor.Product = [..newValues];
        }

        if (service != null)
        {
            var newValues = await Context.Service
                .Where(x => service.Select(y => y.Id).Contains(x.Id))
                .ToListAsync();
            vendor.Service = [..newValues];
        }
    }

    protected override async Task OnBeforeUpdateAsync(Vendor existing, Vendor update)
    {
        await Context.Entry(existing).Collection(x => x.Product).LoadAsync();
        await Context.Entry(existing).Collection(x => x.Service).LoadAsync();

        if (update.Product != null)
        {
            var updateValues = update.Product.Select(x => x.Id);
            existing.Product ??= [];
            existing.Product.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !existing.Product.Select(y => y.Id).Contains(x));
            var newValues = await Context.Product.Where(x => addValues.Contains(x.Id)).ToListAsync();
            existing.Product.AddRange(newValues);
        }

        if (update.Service != null)
        {
            var updateValues = update.Service.Select(x => x.Id);
            existing.Service ??= [];
            existing.Service.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !existing.Service.Select(y => y.Id).Contains(x));
            var newValues = await Context.Service.Where(x => addValues.Contains(x.Id)).ToListAsync();
            existing.Service.AddRange(newValues);
        }
    }
}
