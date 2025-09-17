using Hangfire;
using Microsoft.AspNetCore.Identity;
using Vanigam.CRM.Server.Permissions;

namespace Vanigam.CRM.Server.HangFire
{
    public class ReconcilePermissionJob(ILogger<ReconcilePermissionJob> logger, IServiceProvider serviceProvider)
    {
        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task ReconcilePermission()
        {
            try
            {
                using IServiceScope scope = ServiceProvider.CreateScope();
                ReconcilePermissionService service = scope.ServiceProvider.GetRequiredService<ReconcilePermissionService>();
                await service.ReconcilePermission();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}

