using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class InvoiceItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<InvoiceItem>> logger)
    : BaseService<InvoiceItem>(context, logger)
{
    public override DbSet<InvoiceItem> GetDbSet()
    {
        return Context.InvoiceItems;
    }
}
