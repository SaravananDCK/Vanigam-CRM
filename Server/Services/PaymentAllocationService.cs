using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PaymentAllocationService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PaymentAllocation>> logger)
    : BaseService<PaymentAllocation>(context, logger)
{
    public override DbSet<PaymentAllocation> GetDbSet()
    {
        return Context.PaymentAllocations;
    }
}
