using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class AuditLogService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<AuditLog>> logger)
    : BaseService<AuditLog>(context, logger)
{
    public override DbSet<AuditLog> GetDbSet()
    {
        return Context.AuditLogs;
    }
}