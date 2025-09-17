using System.Linq.Dynamic.Core;
using System.Text;
using Alexinea.FastMember;
using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Enums;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services.Reporting
{
    public class ReportService()
    {

        public async Task<FileDocument> CreateReportFileDocument(VanigamAccountingDbContext Context, ReportParameterDto parameter)
        {
            //var reportTemplate = Context?.ReportTemplates?.FirstOrDefault(r => r.Name == parameter.ReportName);
            //if (reportTemplate != null)
            //{
            //    var xtraReport = await GetXtraReport(Context, reportTemplate, parameter.Criteria, parameter.Title, parameter.Parameters);
            //    if (xtraReport != null)
            //    {
            //        using MemoryStream ms = new MemoryStream();
            //        await xtraReport.ExportToPdfAsync(ms, new PdfExportOptions() { ShowPrintDialogOnOpen = true });
            //        ms.Seek(0, SeekOrigin.Begin);

            //        var tenantId = parameter.TenantId ?? Context?.Patients?.Where(p => p.Oid == parameter.PatientId).Select(p => p.TenantId).FirstOrDefault();
            //        if (tenantId != null)
            //        {
            //            var pdfContent = "data:application/pdf;base64," + Convert.ToBase64String(ms.ToArray());
            //            var category = Context?.FileCategories?.FirstOrDefault(c => c.Name == parameter.Category);
            //            if (category == null && !string.IsNullOrEmpty(parameter.Category))
            //            {
            //                category = new FileCategory() { Oid = Guid.NewGuid(), Name = parameter.Category };
            //                Context?.FileCategories?.Add(category);
            //            }
            //            var fileDocument = new FileDocument()
            //            {
            //                Oid = Guid.NewGuid(),
            //                FileName = parameter.FileName,
            //                FileSize = System.Text.Encoding.ASCII.GetByteCount(pdfContent),
            //                FileType = FileTypes.PDF,
            //                Content = pdfContent,
            //                CategoryId = category?.Oid,
            //                TenantId = tenantId
            //            };
            //            Context.FileDocuments?.Add(fileDocument);

            //            await Context.SaveChangesAsync();
            //            await AzureFileDocumentService.UploadFileAsync(fileDocument);
            //            return fileDocument;
            //        }
            //    }
            //}
            return null;
        }
        public async Task<FileDocument> UpdateReportFileDocument(VanigamAccountingDbContext Context, Guid? fileDocumentId, ReportParameterDto parameter)
        {
            //var fileDocument = Context.FileDocuments.FirstOrDefault(f => f.Oid == fileDocumentId);
            //if (fileDocument != null)
            //{
            //    var reportTemplate = Context?.ReportTemplates?.FirstOrDefault(r => r.Name == parameter.ReportName);
            //    if (reportTemplate != null)
            //    {
            //        var xtraReport = await GetXtraReport(Context, reportTemplate, parameter.Criteria, parameter.Title, parameter.Parameters);
            //        if (xtraReport != null)
            //        {
            //            using MemoryStream ms = new MemoryStream();
            //            await xtraReport.ExportToPdfAsync(ms, new PdfExportOptions() { ShowPrintDialogOnOpen = true });
            //            ms.Seek(0, SeekOrigin.Begin);

            //            var tenantId = parameter.TenantId ?? Context?.Patients?.Where(p => p.Oid == parameter.PatientId).Select(p => p.TenantId).FirstOrDefault();
            //            if (tenantId != null)
            //            {
            //                var pdfContent = "data:application/pdf;base64," + Convert.ToBase64String(ms.ToArray());
            //                var category = Context?.FileCategories?.FirstOrDefault(c => c.Name == parameter.Category);
            //                if (category == null && !string.IsNullOrEmpty(parameter.Category))
            //                {
            //                    category = new FileCategory() { Oid = Guid.NewGuid(), Name = parameter.Category };
            //                    Context?.FileCategories?.Add(category);
            //                }
            //                fileDocument.Content = pdfContent;
            //                Context.FileDocuments?.Update(fileDocument);
            //                await Context.SaveChangesAsync();
            //                await AzureFileDocumentService.UploadFileAsync(fileDocument);
            //                return fileDocument;
            //            }
            //        }
            //    }
            //}
            return null;
        }
        public async Task<byte[]> GetReportPdfBytes(VanigamAccountingDbContext Context, ReportParameterDto parameter)
        {
            var reportTemplate = Context?.ReportTemplates?.FirstOrDefault(r => r.Name == parameter.ReportName);
            if (reportTemplate != null)
            {
                var xtraReport = await GetXtraReport(Context, reportTemplate, parameter.Criteria, parameter.Title, parameter.Parameters);
                if (xtraReport != null)
                {
                    using MemoryStream ms = new MemoryStream();
                    await xtraReport.ExportToPdfAsync(ms, new PdfExportOptions() { ShowPrintDialogOnOpen = true });
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
            }
            return null;
        }

        private static async Task<XtraReport> GetXtraReport(VanigamAccountingDbContext Context, ReportTemplate reportTemplate, string queryStringPart, string title, List<ReportParameterValueDto> parameters)
        {
            try
            {
                var xtraReport = new XtraReport();

                using var ms1 = new MemoryStream(Encoding.ASCII.GetBytes(reportTemplate.Content));
                xtraReport.Name = reportTemplate.Name;
                xtraReport.LoadLayoutFromXml(ms1);
                // Parsing the query string
                var dbSet = Context.GetPropertyValue(reportTemplate.DbSet) as IQueryable<dynamic>;
                ObjectDataSource ds = new ObjectDataSource();
                if (!string.IsNullOrEmpty(queryStringPart))
                {
                    dbSet = dbSet?.AsQueryable().Where(queryStringPart);
                }
                foreach (var expandProperty in reportTemplate.Expands.Split(';'))
                {
                    dbSet = dbSet?.Include(expandProperty);
                }
                ds.DataSource = dbSet?.ToDynamicList();

                if (!string.IsNullOrEmpty(title))
                {
                    foreach (Band reportBand in xtraReport.Bands)
                    {
                        var headerControl = reportBand.FindControl("lblHeader", true) as XRLabel;
                        if (headerControl != null)
                        {
                            headerControl.Text = title;
                            break;
                        }
                    }
                }
                xtraReport.DataSource = ds;
                foreach (var parameter in parameters)
                {
                    xtraReport.Parameters[parameter.Name].Value = parameter.Value;
                }

                return xtraReport;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.Message);
                return null;
            }


        }
    }
}

