using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ActivityService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Activity>> logger)
    : BaseService<Activity>(context, logger)
{
    public override DbSet<Activity> GetDbSet()
    {
        return Context.Activities;
    }
}