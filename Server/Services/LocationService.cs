using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class LocationService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Location>> logger)
    : BaseService<Location>(context, logger)
{
    public override DbSet<Location> GetDbSet()
    {
        return Context.Locations;
    }
}