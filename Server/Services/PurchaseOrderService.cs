using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseOrderService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PurchaseOrder>> logger)
    : BaseService<PurchaseOrder>(context, logger)
{
    public override DbSet<PurchaseOrder> GetDbSet()
    {
        return Context.PurchaseOrders;
    }
}
