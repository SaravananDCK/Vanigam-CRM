using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;

namespace Vanigam.CRM.Server.Controllers
{
    public class CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService)
        : WebDocumentViewerController(controllerService);
}

