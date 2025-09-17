using Hangfire;

using Vanigam.CRM.Server.HangFire;

namespace Vanigam.CRM.Server.Extensions;

public static class HangFireJobExtensions

{
    public static void AddOrUpdateHangFireJobs(this WebApplicationBuilder webApplicationBuilder)
    {
        RecurringJob.AddOrUpdate<ReconcileReportsTemplateJob>(nameof(ReconcileReportsTemplateJob), job => job.ReconcileReports(), Cron.Never);
        RecurringJob.AddOrUpdate<ReconcilePermissionJob>(nameof(ReconcilePermissionJob), job => job.ReconcilePermission(), Cron.Never);
    }
}
