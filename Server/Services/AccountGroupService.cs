using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class AccountGroupService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<AccountGroup>> logger)
    : BaseService<AccountGroup>(context, logger)
{
    public override DbSet<AccountGroup> GetDbSet()
    {
        return Context.AccountGroups;
    }
}
