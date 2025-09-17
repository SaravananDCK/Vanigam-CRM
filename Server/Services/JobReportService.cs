using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class JobReportService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<JobReport>> logger)
    : BaseService<JobReport>(context, logger)
{
    public override DbSet<JobReport> GetDbSet()
    {
        return Context.JobReports;
    }
}