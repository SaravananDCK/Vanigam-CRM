using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class VehicleService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Vehicle>> logger)
    : BaseService<Vehicle>(context, logger)
{
    public override DbSet<Vehicle> GetDbSet()
    {
        return Context.Vehicles;
    }
}