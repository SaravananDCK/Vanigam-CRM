using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class StockLedgerEntryService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<StockLedgerEntry>> logger)
    : BaseService<StockLedgerEntry>(context, logger)
{
    public override DbSet<StockLedgerEntry> GetDbSet()
    {
        return Context.StockLedgerEntries;
    }
}
