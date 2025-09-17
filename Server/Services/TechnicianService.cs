using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class TechnicianService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Technician>> logger)
    : BaseService<Technician>(context, logger)
{
    public override DbSet<Technician> GetDbSet()
    {
        return Context.Technicians;
    }
}