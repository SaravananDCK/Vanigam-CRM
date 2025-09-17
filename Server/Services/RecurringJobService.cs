using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class RecurringJobService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<RecurringJob>> logger)
    : BaseService<RecurringJob>(context, logger)
{
    public override DbSet<RecurringJob> GetDbSet()
    {
        return Context.RecurringJobs;
    }
}