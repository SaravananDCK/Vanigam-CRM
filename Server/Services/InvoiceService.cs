using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class InvoiceService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Invoice>> logger)
    : BaseService<Invoice>(context, logger)
{
    public override DbSet<Invoice> GetDbSet()
    {
        return Context.Invoices;
    }
}