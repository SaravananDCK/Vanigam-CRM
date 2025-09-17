using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class JobAssignmentService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<JobAssignment>> logger)
    : BaseService<JobAssignment>(context, logger)
{
    public override DbSet<JobAssignment> GetDbSet()
    {
        return Context.JobAssignments;
    }
}