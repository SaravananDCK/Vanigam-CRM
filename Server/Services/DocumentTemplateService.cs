using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Json;
using Alexinea.FastMember;
using DevExpress.Data.Filtering;
using DevExpress.Pdf;
using DevExpress.Xpo;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Native;
using DevExpress.XtraReports.UI;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NGS.Templater;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Server.Services;

public class DocumentTemplateService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<DocumentTemplate>> logger, IWebHostEnvironment webHostEnvironment)
    : BaseService<DocumentTemplate>(context, logger)
{
    public IWebHostEnvironment Environment { get; } = webHostEnvironment;

    public override DbSet<DocumentTemplate> GetDbSet()
    {
        return Context.DocumentTemplates;
    }
    public async Task ReconcileDocx()
    {
        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\DocxTemplateData.json");
        var reportTemplates = JsonConvert.DeserializeObject<List<DocxTemplate>>(await File.ReadAllTextAsync(jsonFilePath));
        var templatesPath = Path.Combine(Environment.WebRootPath, "templates");
        foreach (var template in reportTemplates)
        {
            var fi = new FileInfo($"{template.FileName}");
            // Example: Accessing a specific template file
            var templateFilePath = Path.Combine(templatesPath, fi.Name);
            var documentTemplate = Context?.DocumentTemplates?.FirstOrDefault(t => t.Name == template.Name && t.TemplateType == TemplateTypes.DocxTemplate );
            template.Content = Convert.ToBase64String(await File.ReadAllBytesAsync(templateFilePath));
            if (documentTemplate == null)
            {
                Context?.DocumentTemplates?.Add(template);
            }
            else
            {
                documentTemplate.Content = template.Content;
                if (documentTemplate is DocxTemplate docxTemplate)
                {
                    docxTemplate.Expands = template.Expands;
                    docxTemplate.DbSet = template.DbSet;
                }
                Context?.DocumentTemplates?.Update(documentTemplate);
            }
        }
        await Context.SaveChangesAsync();
        jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\DocxMacroTemplateData.json");
        var reportTemplates2 = JsonConvert.DeserializeObject<List<DocxMacroTemplate>>(await File.ReadAllTextAsync(jsonFilePath));
        foreach (var template in reportTemplates2)
        {
            var fi = new FileInfo($"{template.FileName}");
            var templateFilePath = Path.Combine(templatesPath, fi.Name);
            var documentTemplate = Context?.DocumentTemplates?.FirstOrDefault(t => t.Name == template.Name && t.TemplateType == TemplateTypes.DocxMacroTemplate);
            template.Content = Convert.ToBase64String(await File.ReadAllBytesAsync(templateFilePath));
            if (documentTemplate == null)
            {
                Context?.DocumentTemplates?.Add(template);
            }
            else
            {
                documentTemplate.Content = template.Content;
                if (documentTemplate is DocxMacroTemplate docxTemplate)
                {
                    docxTemplate.Expands = template.Expands;
                    docxTemplate.DbSet = template.DbSet;
                }
                Context?.DocumentTemplates?.Update(documentTemplate);
            }
        }
        await Context.SaveChangesAsync();
    }
    public async Task ReconcileReports()
    {
        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\ReportsTemplateData.json");
        var reportTemplates = JsonConvert.DeserializeObject<List<ReportTemplate>>(await File.ReadAllTextAsync(jsonFilePath));
        foreach (var template in reportTemplates)
        {
            var documentTemplate = Context.DocumentTemplates.FirstOrDefault(t => t.Name == template.Name);
            string contentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Reports\", $"{template.Name}.repx");
            if (File.Exists(contentFilePath))
            {
                template.Content = await File.ReadAllTextAsync(contentFilePath);
            }
            if (documentTemplate == null)
            {
                Context.DocumentTemplates.Add(template);
            }
            else
            {
                documentTemplate.Content = template.Content;
                if (documentTemplate is ReportTemplate reportTemplate)
                {
                    reportTemplate.Expands = template.Expands;
                    reportTemplate.DbSet = template.DbSet;
                }

                Context.DocumentTemplates.Update(documentTemplate);
            }
        }
        await Context.SaveChangesAsync();
    }
    public async Task ReconcilePDFDocuments()
    {
        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\PdfTemplateData.json");
        var pdfTemplates = JsonConvert.DeserializeObject<List<PdfTemplate>>(await File.ReadAllTextAsync(jsonFilePath));
        foreach (var template in pdfTemplates)
        {
            var documentTemplate = Context.DocumentTemplates.FirstOrDefault(t => t.Name == template.Name);
            if (documentTemplate == null)
            {
                Context.DocumentTemplates.Add(template);
            }
            else
            {
                documentTemplate.Content = template.Content;
                Context.DocumentTemplates.Update(documentTemplate);
            }
        }
        await Context.SaveChangesAsync();


        jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\PdfFieldData.json");
        var pdfFields = JsonConvert.DeserializeObject<List<PdfField>>(await File.ReadAllTextAsync(jsonFilePath));
        foreach (var pdfField in pdfFields)
        {
            var existingPdfField = Context.PdfFields.FirstOrDefault(t => t.FieldName == pdfField.FieldName && t.TemplateId == pdfField.TemplateId);
            if (existingPdfField == null)
            {
                Context.PdfFields.Add(pdfField);
            }
            else
            {
                existingPdfField.FieldName = pdfField.FieldName;
                existingPdfField.MappedProperty = pdfField.MappedProperty;
                Context.PdfFields.Update(existingPdfField);
            }
        }
        await Context.SaveChangesAsync();

        jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\SignPdfFieldData.json");
        var signPdfFields = JsonConvert.DeserializeObject<List<SignPdfField>>(await File.ReadAllTextAsync(jsonFilePath));
        foreach (var signPdfField in signPdfFields)
        {
            var existingSignPdfField = Context.SignPdfFields.FirstOrDefault(t => t.MappedProperty == signPdfField.MappedProperty && t.TemplateId == signPdfField.TemplateId);
            if (existingSignPdfField == null)
            {
                Context.SignPdfFields.Add(signPdfField);
            }
            else
            {
                existingSignPdfField.Top = signPdfField.Top;
                existingSignPdfField.Left = signPdfField.Left;
                existingSignPdfField.Height = signPdfField.Height;
                existingSignPdfField.Width = signPdfField.Width;
                existingSignPdfField.Page = signPdfField.Page;
                existingSignPdfField.MappedProperty = signPdfField.MappedProperty;
                Context.SignPdfFields.Update(existingSignPdfField);
            }
        }
        await Context.SaveChangesAsync();
    }
   public async Task<byte[]> GenerateDocument(ReportParameterDto parameter)
    {
        var documentTemplate = Context?.DocumentTemplates?.FirstOrDefault(r => r.Name == parameter.ReportName);
        var dbSet = Context.GetPropertyValue(documentTemplate?.DbSet) as IQueryable<dynamic>;
        if (!string.IsNullOrEmpty(parameter.Criteria))
        {
            dbSet = dbSet?.AsQueryable().Where(parameter.Criteria);
        }

        dbSet = documentTemplate?.Expands.Split(';').Aggregate(dbSet, (current, expandProperty) => current?.Include(expandProperty));
        var list = dbSet?.ToDynamicList().ToList();
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true // Optional: for better readability of the output JSON
        };

        //var ms = Process(new FileInfo($"{parameter.FileName}"), System.Text.Json.JsonSerializer.Serialize(list, options), true, false);
        var ms = await Process(parameter, documentTemplate, DevExpress.XtraRichEdit.DocumentFormat.OpenXml, list, true, false);

        return ms;
    }
    async Task<byte[]> Process(ReportParameterDto parameter, DocumentTemplate documentTemplate, DevExpress.XtraRichEdit.DocumentFormat documentFormat, List<dynamic> argument, bool asSchema, bool debugLog)
    {
        // Build the path to the templates folder
        var templatesPath = Path.Combine(Environment.WebRootPath, "templates");
        var fi = new FileInfo($"{parameter.FileName}");
        // Example: Accessing a specific template file
        var templateFilePath = Path.Combine(templatesPath, fi.Name);

        var cts = new CancellationTokenSource();
        //if processing does not finish within specified timeout (default 30 seconds),
        //cancel the run which will throw OperationCancelledException
        var ms = new MemoryStream();
        var documentFactory = Configuration.Factory;
        await using (var input = new MemoryStream(Convert.FromBase64String(documentTemplate.Content)))
        using (var doc = documentFactory.Open(input, fi.Extension, ms, cts.Token))
        {
            doc.Process(argument);
        }
        ms.Position = 0;
        RichEditDocumentServer richEditDocumentServer = new RichEditDocumentServer();
        richEditDocumentServer.LoadDocument(ms, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
        richEditDocumentServer.Document.ReplaceAll("Unlicensed version. Please register @ templater.info", "", SearchOptions.None);

        var result = richEditDocumentServer.SaveDocument(documentFormat);
        await ms.DisposeAsync();
        richEditDocumentServer.Dispose();
        return result;
    }
    public async Task<byte[]> GenerateDocumentPdf(ReportParameterDto parameter)
    {
        var documentTemplate = Context?.DocumentTemplates?.FirstOrDefault(r => r.Name == parameter.ReportName);
        var dbSet = Context.GetPropertyValue(documentTemplate?.DbSet) as IQueryable<dynamic>;
        if (!string.IsNullOrEmpty(parameter.Criteria))
        {
            dbSet = dbSet?.AsQueryable().Where(parameter.Criteria);
        }

        dbSet = documentTemplate?.Expands.Split(';').Aggregate(dbSet, (current, expandProperty) => current?.Include(expandProperty));
        var list = dbSet?.ToDynamicList().ToList();
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true // Optional: for better readability of the output JSON
        };

        //var ms = Process(new FileInfo($"{parameter.FileName}"), System.Text.Json.JsonSerializer.Serialize(list, options), true, false);
        var ms = await ProcessPdf(parameter, documentTemplate, DevExpress.XtraRichEdit.DocumentFormat.OpenXml, list, true, false);

        return ms;
    }
    async Task<byte[]> ProcessPdf(ReportParameterDto parameter, DocumentTemplate documentTemplate, DevExpress.XtraRichEdit.DocumentFormat documentFormat, List<dynamic> argument, bool asSchema, bool debugLog)
    {
        // Build the path to the templates folder
        var templatesPath = Path.Combine(Environment.WebRootPath, "templates");
        var fi = new FileInfo($"{parameter.FileName}");
        // Example: Accessing a specific template file
        var templateFilePath = Path.Combine(templatesPath, fi.Name);

        var cts = new CancellationTokenSource();
        //if processing does not finish within specified timeout (default 30 seconds),
        //cancel the run which will throw OperationCancelledException
        var ms = new MemoryStream();
        var documentFactory = Configuration.Factory;
        await using (var input = new MemoryStream(Convert.FromBase64String(documentTemplate.Content)))
        using (var doc = documentFactory.Open(input, fi.Extension, ms, cts.Token))
        {
            doc.Process(argument);
        }
        ms.Position = 0;
        RichEditDocumentServer richEditDocumentServer = new RichEditDocumentServer();
        richEditDocumentServer.LoadDocument(ms, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
        richEditDocumentServer.Document.ReplaceAll("Unlicensed version. Please register @ templater.info", "", SearchOptions.None);

        var ms2 = new MemoryStream();
        richEditDocumentServer.ExportToPdf(ms2);
        ms2.Position = 0;
        var result= ms2.ToArray();
        await ms2.DisposeAsync();
        await ms.DisposeAsync();
        richEditDocumentServer.Dispose();
        return result;
    }
    
}
