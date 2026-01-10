using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services.Pdf;

public abstract class BasePdfService<T>(
    VanigamAccountingDbContext context,
    ILogger<BasePdfService<T>> logger,
    ICurrentUserService currentUserService) where T : BaseClass
{
    protected VanigamAccountingDbContext Context = context;
    protected ILogger<BasePdfService<T>> Logger = logger;
    protected ICurrentUserService CurrentUserService = currentUserService;

    public abstract Task<T> GetEntityWithIncludesAsync(Guid entityId);
    public abstract string GetDocumentTitle();
    public abstract Color GetDocumentColor();
    public abstract void BuildDocumentHeader(T entity, ColumnDescriptor column);
    public abstract void BuildDocumentContent(T entity, ColumnDescriptor column);
    public abstract void BuildDocumentFooter(T entity, ColumnDescriptor column);

    public async Task<byte[]> GeneratePdfAsync(Guid entityId)
    {
        Logger.LogInformation("Generating PDF for {EntityType} {EntityId}", typeof(T).Name, entityId);

        var entity = await GetEntityWithIncludesAsync(entityId);
        if (entity == null)
        {
            Logger.LogWarning("{EntityType} {EntityId} not found", typeof(T).Name, entityId);
            throw new ArgumentException($"{typeof(T).Name} with ID {entityId} not found");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header()
                    .Text(GetDocumentTitle())
                    .SemiBold().FontSize(24).FontColor(GetDocumentColor());

                // Content
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        BuildDocumentHeader(entity, column);
                        BuildDocumentContent(entity, column);
                    });

                // Footer
                page.Footer()
                    .Column(column =>
                    {
                        BuildDocumentFooter(entity, column);

                        column.Item()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generated on ");
                                x.Span(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture)).SemiBold();
                            });
                    });
            });
        });

        return document.GeneratePdf();
    }

    protected void BuildCompanyInfo(ColumnDescriptor column)
    {
        var tenantId = CurrentUserService.TenantId;
        var settings = Context.TenantAccountingSettings
            .FirstOrDefault(s => s.TenantId == tenantId);

        column.Item().Text("From:").SemiBold();

        if (settings != null)
        {
            if (!string.IsNullOrEmpty(settings.CompanyName))
                column.Item().Text(settings.CompanyName);

            if (!string.IsNullOrEmpty(settings.CompanyAddress))
                column.Item().Text(settings.CompanyAddress);

            var cityLine = BuildCityStateZip(settings.CompanyCity, settings.CompanyState, settings.CompanyPostalCode);
            if (!string.IsNullOrEmpty(cityLine))
                column.Item().Text(cityLine);

            if (!string.IsNullOrEmpty(settings.CompanyCountry))
                column.Item().Text(settings.CompanyCountry);

            if (!string.IsNullOrEmpty(settings.CompanyPhone))
                column.Item().Text($"Phone: {settings.CompanyPhone}");

            if (!string.IsNullOrEmpty(settings.CompanyEmail))
                column.Item().Text($"Email: {settings.CompanyEmail}");

            if (!string.IsNullOrEmpty(settings.CompanyWebsite))
                column.Item().Text($"Web: {settings.CompanyWebsite}");

            if (!string.IsNullOrEmpty(settings.CompanyTaxId))
                column.Item().Text($"Tax ID: {settings.CompanyTaxId}");
        }
        else
        {
            column.Item().Text("Company Name");
            column.Item().Text("Company Address");
            column.Item().Text("City, State ZIP");
            column.Item().Text("Phone: Company Phone");
            column.Item().Text("Email: Company Email");
        }
    }

    private string BuildCityStateZip(string city, string state, string postalCode)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(city))
            parts.Add(city);

        if (!string.IsNullOrEmpty(state))
            parts.Add(state);

        var result = string.Join(", ", parts);

        if (!string.IsNullOrEmpty(postalCode))
        {
            if (!string.IsNullOrEmpty(result))
                result += " " + postalCode;
            else
                result = postalCode;
        }

        return result;
    }

    protected void BuildCustomerInfo(LedgerAccount customer, ColumnDescriptor column)
    {
        if (customer == null) return;

        column.Item().Text("Bill To:").SemiBold();
        column.Item().Text(customer.Name);
        if (!string.IsNullOrEmpty(customer.Address))
            column.Item().Text(customer.Address);
        if (!string.IsNullOrEmpty(customer.City))
        {
            var addressLine = customer.City;
            if (!string.IsNullOrEmpty(customer.State))
                addressLine += $", {customer.State}";
            if (!string.IsNullOrEmpty(customer.PostalCode))
                addressLine += $" {customer.PostalCode}";
            column.Item().Text(addressLine);
        }

        if (!string.IsNullOrEmpty(customer.Email))
            column.Item().Row(row =>
            {
                row.ConstantItem(35).Text("Email:").SemiBold();
                row.AutoItem().Text(customer.Email);
            });
        if (!string.IsNullOrEmpty(customer.Phone))
            column.Item().Row(row =>
            {
                row.ConstantItem(35).Text("Phone:").SemiBold();
                row.AutoItem().Text(customer.Phone);
            });
    }

    protected void BuildItemsTable<TItem>(IEnumerable<TItem> items, ColumnDescriptor column,
        Func<TItem, double> getQuantity,
        Func<TItem, string> getDescription,
        Func<TItem, decimal> getUnitPrice) where TItem : class
    {
        if (!items.Any()) return;

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();   // Description
                columns.ConstantColumn(50);  // Qty
                columns.ConstantColumn(80); // Unit Price
                columns.ConstantColumn(80); // Total
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Item Name").SemiBold();
                header.Cell().Element(CellStyle).PaddingRight(10).AlignRight().Text("Qty").SemiBold();
                header.Cell().Element(CellStyle).AlignRight().Text("Unit Price").SemiBold();
                header.Cell().Element(CellStyle).AlignRight().Text("Total").SemiBold();

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold())
                    .MinHeight(20)
                    .PaddingVertical(1)
                    .BorderBottom(1).BorderTop(1)
                    .BorderColor(Colors.Black)
                    .AlignMiddle();
                }
            });

            // Items
            foreach (var item in items)
            {
                var quantity = getQuantity(item);
                var unitPrice = getUnitPrice(item);
                var total = (decimal)quantity * unitPrice;

                table.Cell().Element(CellStyle).Text(getDescription(item));
                table.Cell().Element(CellStyle).PaddingRight(10).AlignRight().Text(quantity.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text($"${unitPrice:F2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"${total:F2}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.PaddingVertical(1)
                    .MinHeight(20)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .AlignMiddle();
                }
            }
        });
    }
}