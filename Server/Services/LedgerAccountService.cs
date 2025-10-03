using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class LedgerAccountService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<LedgerAccount>> logger)
    : BaseService<LedgerAccount>(context, logger)
{
    public override DbSet<LedgerAccount> GetDbSet()
    {
        return Context.LedgerAccounts;
    }
}
