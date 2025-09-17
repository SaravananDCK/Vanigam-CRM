using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class SlaService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Sla>> logger)
    : BaseService<Sla>(context, logger)
{
    public override DbSet<Sla> GetDbSet()
    {
        return Context.Slas;
    }
}