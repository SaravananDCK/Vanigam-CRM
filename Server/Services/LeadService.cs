using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class LeadService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Lead>> logger)
    : BaseService<Lead>(context, logger)
{
    public override DbSet<Lead> GetDbSet()
    {
        return Context.Leads;
    }
}