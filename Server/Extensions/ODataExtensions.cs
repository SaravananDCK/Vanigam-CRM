using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Server.Extensions
{
    public static class ODataExtensions
    {
        public static void InitOData(this ODataOptions opt)
        {
            var oDataBuilderVanigamAccounting = new ODataConventionModelBuilder();

            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.ApplicationTenant>(nameof(VanigamAccountingDbContext.Tenants));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.ApplicationTenantUser>(nameof(VanigamAccountingDbContext.ApplicationTenantUsers));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Language>(nameof(VanigamAccountingDbContext.Languages));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.FileCategory>(nameof(VanigamAccountingDbContext.FileCategories));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.FileDocument>(nameof(VanigamAccountingDbContext.FileDocuments));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.DocumentTemplate>(nameof(VanigamAccountingDbContext.DocumentTemplates));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.ReportTemplate>(nameof(VanigamAccountingDbContext.ReportTemplates));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.PdfField>(nameof(VanigamAccountingDbContext.PdfFields));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.SignPdfField>(nameof(VanigamAccountingDbContext.SignPdfFields));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.DocxMacroTemplate>(nameof(VanigamAccountingDbContext.DocxMacroTemplates));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.DocxTemplate>(nameof(VanigamAccountingDbContext.DocxTemplates));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.PdfTemplate>(nameof(VanigamAccountingDbContext.PdfTemplates));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.UserSession>(nameof(VanigamAccountingDbContext.UserSessions));

            // CRM Entities
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Lead>(nameof(VanigamAccountingDbContext.Leads));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Opportunity>(nameof(VanigamAccountingDbContext.Opportunities));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Activity>(nameof(VanigamAccountingDbContext.Activities));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Customer>(nameof(VanigamAccountingDbContext.Customers));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Contact>(nameof(VanigamAccountingDbContext.Contacts));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Technician>(nameof(VanigamAccountingDbContext.Technicians));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Job>(nameof(VanigamAccountingDbContext.Jobs));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.JobAssignment>(nameof(VanigamAccountingDbContext.JobAssignments));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Appointment>(nameof(VanigamAccountingDbContext.Appointments));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.TimeSheet>(nameof(VanigamAccountingDbContext.TimeSheets));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Invoice>(nameof(VanigamAccountingDbContext.Invoices));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Item>(nameof(VanigamAccountingDbContext.Items));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Product>(nameof(VanigamAccountingDbContext.Products));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.InventoryItem>(nameof(VanigamAccountingDbContext.InventoryItems));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.ServiceItem>(nameof(VanigamAccountingDbContext.ServiceItems));

            //oDataBuilderVanigamAccounting.EntityType<InventoryItem>().DerivesFrom<Item>();
            //oDataBuilderVanigamAccounting.EntityType<Product>().DerivesFrom<Item>();
            //oDataBuilderVanigamAccounting.EntityType<ServiceItem>().DerivesFrom<Item>();

            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.MaterialUsage>(nameof(VanigamAccountingDbContext.MaterialUsages));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Quote>(nameof(VanigamAccountingDbContext.Quotes));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.QuoteItem>(nameof(VanigamAccountingDbContext.QuoteItems));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Payment>(nameof(VanigamAccountingDbContext.Payments));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.JobReport>(nameof(VanigamAccountingDbContext.JobReports));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Attachment>(nameof(VanigamAccountingDbContext.Attachments));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.GPSPoint>(nameof(VanigamAccountingDbContext.GPSPoints));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Contract>(nameof(VanigamAccountingDbContext.Contracts));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Sla>(nameof(VanigamAccountingDbContext.Slas));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.RecurringJob>(nameof(VanigamAccountingDbContext.RecurringJobs));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Location>(nameof(VanigamAccountingDbContext.Locations));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Vehicle>(nameof(VanigamAccountingDbContext.Vehicles));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Feedback>(nameof(VanigamAccountingDbContext.Feedbacks));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Notification>(nameof(VanigamAccountingDbContext.Notifications));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.AuditLog>(nameof(VanigamAccountingDbContext.AuditLogs));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.CustomField>(nameof(VanigamAccountingDbContext.CustomFields));

            // Accounting Entities
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.AccountGroup>(nameof(VanigamAccountingDbContext.AccountGroups));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.LedgerAccount>(nameof(VanigamAccountingDbContext.LedgerAccounts));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Vendor>(nameof(VanigamAccountingDbContext.Vendors));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.BankAccount>(nameof(VanigamAccountingDbContext.BankAccounts));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.Voucher>(nameof(VanigamAccountingDbContext.Vouchers));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.VoucherLine>(nameof(VanigamAccountingDbContext.VoucherLines));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.PurchaseOrder>(nameof(VanigamAccountingDbContext.PurchaseOrders));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.PurchaseOrderItem>(nameof(VanigamAccountingDbContext.PurchaseOrderItems));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.PurchaseInvoice>(nameof(VanigamAccountingDbContext.PurchaseInvoices));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.PurchaseInvoiceItem>(nameof(VanigamAccountingDbContext.PurchaseInvoiceItems));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.LedgerEntry>(nameof(VanigamAccountingDbContext.LedgerEntries));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.StockLedgerEntry>(nameof(VanigamAccountingDbContext.StockLedgerEntries));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.InvoiceItem>(nameof(VanigamAccountingDbContext.InvoiceItems));
            oDataBuilderVanigamAccounting.EntitySet<Vanigam.CRM.Objects.Entities.NumberSeries>(nameof(VanigamAccountingDbContext.NumberSeries));

            oDataBuilderVanigamAccounting.EntitySet<ApplicationUser>("ApplicationUsers");
            var usersType = oDataBuilderVanigamAccounting.StructuralTypes.First(x => x.ClrType == typeof(ApplicationUser));
            usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Password)));
            usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.ConfirmPassword)));
            oDataBuilderVanigamAccounting.EntitySet<ApplicationRole>("ApplicationRoles");
            oDataBuilderVanigamAccounting.EntitySet<ApplicationTenantUser>("ApplicationTenantUsers");
            var entity = oDataBuilderVanigamAccounting.EntitySet<ApplicationTenant>("ApplicationTenants");
            var functionRefreshSummary = entity.EntityType.Collection.Function("GetTenantUsers")
                .ReturnsCollectionFromEntitySet<ApplicationUser>("ApplicationUsers");
            functionRefreshSummary.Parameter<int>("tenantId");

            opt.AddRouteComponents("odata/VanigamAccountingService", oDataBuilderVanigamAccounting.GetEdmModel(), collection => collection.AddSingleton<ODataUriResolver>(sp => new StringAsEnumResolver() { EnableCaseInsensitive = true })).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null).TimeZone = TimeZoneInfo.Utc;
        }
    }
}

