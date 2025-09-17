using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class GPSPointService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<GPSPoint>> logger)
    : BaseService<GPSPoint>(context, logger)
{
    public override DbSet<GPSPoint> GetDbSet()
    {
        return Context.GPSPoints;
    }
}