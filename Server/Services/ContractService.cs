using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ContractService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Contract>> logger)
    : BaseService<Contract>(context, logger)
{
    public override DbSet<Contract> GetDbSet()
    {
        return Context.Contracts;
    }
}