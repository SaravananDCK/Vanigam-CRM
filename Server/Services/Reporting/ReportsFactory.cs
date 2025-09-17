using DevExpress.XtraReports.UI;

namespace Vanigam.CRM.Server.Services.Reporting;

public static class ReportsFactory
{
    static ReportsFactory()
    {
        
    }

    
    public static Dictionary<string, Func<XtraReport>> Reports = new Dictionary<string, Func<XtraReport>>()
    {
        
    };
}
