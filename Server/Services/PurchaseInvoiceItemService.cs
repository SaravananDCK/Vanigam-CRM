using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseInvoiceItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PurchaseInvoiceItem>> logger)
    : BaseService<PurchaseInvoiceItem>(context, logger)
{
    public override DbSet<PurchaseInvoiceItem> GetDbSet()
    {
        return Context.PurchaseInvoiceItems;
    }
}
