using System.Collections.Specialized;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Web;
using Alexinea.FastMember;
using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraReports;
using DevExpress.XtraReports.Native;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services.Reporting
{
    public interface IObjectDataSourceInjector
    {
        public void Process(XtraReport report);
    }

    class ObjectDataSourceInjector(IServiceProvider serviceProvider) : IObjectDataSourceInjector
    {
        IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public void Process(XtraReport report)
        {
            foreach (var ods in DataSourceManager.GetDataSources<ObjectDataSource>(report, includeSubReports: true))
            {
                if (ods.DataSource is Type dataSourceType)
                {
                    ods.DataSource = ServiceProvider.GetRequiredService(dataSourceType);
                }
            }
        }
    }
    class WebDocumentViewerReportResolver(
        ReportStorageWebExtension reportStorageWebExtension,
        IObjectDataSourceInjector dataSourceInjector)
        : IWebDocumentViewerReportResolver
    {
        IObjectDataSourceInjector DataSourceInjector { get; } = dataSourceInjector ?? throw new ArgumentNullException(nameof(dataSourceInjector));
        ReportStorageWebExtension ReportStorageWebExtension { get; } = reportStorageWebExtension ?? throw new ArgumentNullException(nameof(reportStorageWebExtension));

        public XtraReport Resolve(string reportEntry)
        {
            using (MemoryStream ms = new MemoryStream(ReportStorageWebExtension.GetData(reportEntry)))
            {
                var report = XtraReport.FromStream(ms);
                DataSourceInjector.Process(report);
                return report;
            }
        }
    }
    public class CustomReportStorageWebExtension(VanigamAccountingDbContext dbContext)
        : DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension, IReportStorageTool2
    {
        protected VanigamAccountingDbContext DbContext { get; set; } = dbContext;

        public override bool CanSetData(string url)
        {
            // Determines whether a report with the specified URL can be saved.
            // Add custom logic that returns **false** for reports that should be read-only.
            // Return **true** if no valdation is required.
            // This method is called only for valid URLs (if the **IsValidUrl** method returns **true**).

            return true;
        }

        public override bool IsValidUrl(string url)
        {
            // Determines whether the URL passed to the current report storage is valid.
            // Implement your own logic to prohibit URLs that contain spaces or other specific characters.
            // Return **true** if no validation is required.

            return true;
        }

        public override byte[] GetData(string url)
        {
            // Uses a specified URL to return report layout data stored within a report storage medium.
            // This method is called if the **IsValidUrl** method returns **true**.
            // You can use the **GetData** method to process report parameters sent from the client
            // if the parameters are included in the report URL's query string.
            var splitResults = url.Split('|');
            string reportName = string.Empty;
            string queryStringPart = string.Empty;
            if (splitResults.Length > 1)
            {
                reportName = splitResults[0];
                queryStringPart = splitResults[1];
            }
            else
            {
                reportName = url;
            }
            
            var reportTemplate = DbContext.ReportTemplates.FirstOrDefault(x => x.Name == reportName);
            if (reportTemplate != null)
            {
                return Encoding.ASCII.GetBytes(reportTemplate.Content);
            }


            if (ReportsFactory.Reports.ContainsKey(url))
            {
                using var ms = new MemoryStream();
                using XtraReport report = ReportsFactory.Reports[url]();
                report.SaveLayoutToXml(ms);
                return ms.ToArray();
            }
            throw new DevExpress.XtraReports.Web.ClientControls.FaultException(string.Format("Could not find report '{0}'.", url));
        }

        public void AfterGetData(string url, XtraReport report)
        {
            var splitResults = url.Split('|');
            string reportName = string.Empty;
            string queryStringPart = string.Empty;
            string title = string.Empty;
            if (splitResults.Length > 1)
            {
                reportName = splitResults[0];
                queryStringPart = splitResults[1];
                if (splitResults.Length > 2)
                {
                    title = splitResults[2];
                }
            }
            else
            {
                reportName = url;
            }
            var reportTemplate = DbContext.ReportTemplates.FirstOrDefault(x => x.Name == reportName);

            // Parsing the query string
            var dbSet = DbContext.GetPropertyValue(reportTemplate.DbSet) as IQueryable<dynamic>;
            ObjectDataSource ds = new ObjectDataSource();
            if (!string.IsNullOrEmpty(queryStringPart))
            {
                dbSet = dbSet.AsQueryable().Where(queryStringPart);
            }
            foreach (var expandProperty in reportTemplate.Expands.Split(';'))
            {
                dbSet = dbSet.Include(expandProperty);
            }

            if (!string.IsNullOrEmpty(title))
            {
                foreach (Band reportBand in report.Bands)
                {
                    var headerControl = reportBand.FindControl("lblHeader", true) as XRLabel;
                    if (headerControl != null)
                    {
                        headerControl.Text = title;
                        break;
                    }
                }
            }
            ds.DataSource = dbSet.ToDynamicList();
            report.DataSource = ds;
            report.Tag= DbContext;
        }
        public override Dictionary<string, string> GetUrls()
        {
            // Returns a dictionary that contains the report names (URLs) and display names. 
            // The Report Designer uses this method to populate the Open Report and Save Report dialogs.

            return DbContext.ReportTemplates
                .ToList()
                .Select(x => x.Name)
                .Union(ReportsFactory.Reports.Select(x => x.Key))
                .ToDictionary<string, string>(x => x);
        }

        public override void SetData(XtraReport report, string url)
        {
            // Saves the specified report to the report storage with the specified name
            // (saves existing reports only). 
            using var stream = new MemoryStream(); report.SaveLayoutToXml(stream);
            var reportData = DbContext.ReportTemplates.FirstOrDefault(x => x.Name == url);
            if (reportData == null)
            {
                DbContext.ReportTemplates.Add(new ReportTemplate { Name = url, Content = Encoding.ASCII.GetString(stream.ToArray()) });
            }
            else
            {
                reportData.Content = Encoding.ASCII.GetString(stream.ToArray());
            }
            DbContext.SaveChanges();
        }

        public override string SetNewData(XtraReport report, string defaultUrl)
        {
            // Allows you to validate and correct the specified name (URL).
            // This method also allows you to return the resulting name (URL),
            // and to save your report to a storage. The method is called only for new reports.
            SetData(report, defaultUrl);
            return defaultUrl;
        }
    }
}

