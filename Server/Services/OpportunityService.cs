using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class OpportunityService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Opportunity>> logger)
    : BaseService<Opportunity>(context, logger)
{
    public override DbSet<Opportunity> GetDbSet()
    {
        return Context.Opportunities;
    }
}