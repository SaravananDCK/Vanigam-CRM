using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseOrderItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PurchaseOrderItem>> logger)
    : BaseService<PurchaseOrderItem>(context, logger)
{
    public override DbSet<PurchaseOrderItem> GetDbSet()
    {
        return Context.PurchaseOrderItems;
    }
}
