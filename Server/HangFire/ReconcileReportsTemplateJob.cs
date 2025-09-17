using Hangfire;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.HangFire;

public class ReconcileReportsTemplateJob(ILogger<ReconcileReportsTemplateJob> logger, IServiceProvider serviceProvider)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ReconcileReports()
    {
        try
        {
            using IServiceScope scope = ServiceProvider.CreateScope();
            using DocumentTemplateService service = scope.ServiceProvider.GetRequiredService<DocumentTemplateService>();
            await service.ReconcileReports();
            await service.ReconcilePDFDocuments();
            await service.ReconcileDocx();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}
