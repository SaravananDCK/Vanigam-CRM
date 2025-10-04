using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class CustomerAdvanceService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<CustomerAdvance>> logger)
    : BaseService<CustomerAdvance>(context, logger)
{
    public override DbSet<CustomerAdvance> GetDbSet()
    {
        return Context.CustomerAdvances;
    }
}
