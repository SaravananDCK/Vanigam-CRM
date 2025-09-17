using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class TimeSheetService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<TimeSheet>> logger)
    : BaseService<TimeSheet>(context, logger)
{
    public override DbSet<TimeSheet> GetDbSet()
    {
        return Context.TimeSheets;
    }
}