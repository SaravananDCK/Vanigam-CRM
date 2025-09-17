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

