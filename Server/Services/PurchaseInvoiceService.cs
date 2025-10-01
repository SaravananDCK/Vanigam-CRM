using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseInvoiceService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PurchaseInvoice>> logger)
    : BaseService<PurchaseInvoice>(context, logger)
{
    public override DbSet<PurchaseInvoice> GetDbSet()
    {
        return Context.PurchaseInvoices;
    }
}
