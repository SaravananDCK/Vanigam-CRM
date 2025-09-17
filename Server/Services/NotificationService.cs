using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class NotificationService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Notification>> logger)
    : BaseService<Notification>(context, logger)
{
    public override DbSet<Notification> GetDbSet()
    {
        return Context.Notifications;
    }
}