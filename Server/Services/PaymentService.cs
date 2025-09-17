using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PaymentService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Payment>> logger)
    : BaseService<Payment>(context, logger)
{
    public override DbSet<Payment> GetDbSet()
    {
        return Context.Payments;
    }
}