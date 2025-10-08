using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class BankAccountService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<BankAccount>> logger)
    : BaseService<BankAccount>(context, logger)
{
    public override DbSet<BankAccount> GetDbSet()
    {
        return Context.BankAccounts;
    }
    protected override async Task OnCreatedAsync(BankAccount item)
    {
        if (string.IsNullOrEmpty(item.Code))
        {
            item.Code = await Context.GenerateNextCode(nameof(BankAccount), TenantId);
        }
    }
}
