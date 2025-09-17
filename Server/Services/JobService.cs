using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class JobService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Job>> logger)
    : BaseService<Job>(context, logger)
{
    public override DbSet<Job> GetDbSet()
    {
        return Context.Jobs;
    }
}