using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class ServiceController(ApplicationDbContext context, ILogger<ServiceController> logger)
    : BaseApiController<Service, long>(context, logger)
{
    protected override DbSet<Service> DbSet => Context.Service;
    protected override Expression<Func<Service, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Service";

    protected override IQueryable<Service> GetQueryWithIncludes()
    {
        return Context.Service.Include(x => x.ServiceCategory);
    }

    protected override async Task OnBeforeCreateAsync(Service service)
    {
        var serviceCategory = service.ServiceCategory;
        service.ServiceCategory = null;

        if (serviceCategory != null)
        {
            var newValues = await Context.ServiceCategory
                .Where(x => serviceCategory.Select(y => y.Id).Contains(x.Id))
                .ToListAsync();
            service.ServiceCategory = [..newValues];
        }
    }

    protected override async Task OnBeforeUpdateAsync(Service existing, Service update)
    {
        await Context.Entry(existing).Collection(x => x.ServiceCategory).LoadAsync();

        if (update.ServiceCategory != null)
        {
            var updateValues = update.ServiceCategory.Select(x => x.Id);
            existing.ServiceCategory ??= [];
            existing.ServiceCategory.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !existing.ServiceCategory.Select(y => y.Id).Contains(x));
            var newValues = await Context.ServiceCategory.Where(x => addValues.Contains(x.Id)).ToListAsync();
            existing.ServiceCategory.AddRange(newValues);
        }
    }
}
