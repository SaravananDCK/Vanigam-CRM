using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ContractCoverageRuleService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<ContractCoverageRule>> logger)
    : BaseService<ContractCoverageRule>(context, logger)
{
    public override DbSet<ContractCoverageRule> GetDbSet()
    {
        return Context.ContractCoverageRules;
    }
}
