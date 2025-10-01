using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class LedgerEntryService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<LedgerEntry>> logger)
    : BaseService<LedgerEntry>(context, logger)
{
    public override DbSet<LedgerEntry> GetDbSet()
    {
        return Context.LedgerEntries;
    }
}
