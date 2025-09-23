using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ServiceItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<ServiceItem>> logger)
    : BaseService<ServiceItem>(context, logger)
{
    public override DbSet<ServiceItem> GetDbSet()
    {
        return Context.ServiceItems;
    }
}