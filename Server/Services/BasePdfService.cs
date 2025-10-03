using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Server.Services;

public abstract class BasePdfService<T>(
    VanigamAccountingDbContext context,
    ILogger<BasePdfService<T>> logger) where T : BaseClass
{
    protected VanigamAccountingDbContext Context = context;
    protected ILogger<BasePdfService<T>> Logger = logger;

    public abstract Task<T?> GetEntityWithIncludesAsync(Guid entityId);
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
                                x.Span(DateTime.Now.ToString("MM/dd/yyyy")).SemiBold();
                            });
                    });
            });
        });

        return document.GeneratePdf();
    }

    protected void BuildCompanyInfo(ColumnDescriptor column)
    {
        column.Item().Text("From:").SemiBold();
        column.Item().Text("Your Company Name");
        column.Item().Text("Your Address");
        column.Item().Text("City, State ZIP");
        column.Item().Text("Phone: Your Phone");
        column.Item().Text("Email: Your Email");
    }

    protected void BuildCustomerInfo(LedgerAccount? customer, ColumnDescriptor column)
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
            column.Item().Text($"Email: {customer.Email}");
        if (!string.IsNullOrEmpty(customer.Phone))
            column.Item().Text($"Phone: {customer.Phone}");
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
                columns.ConstantColumn(50);  // Qty
                columns.RelativeColumn();   // Description
                columns.ConstantColumn(80); // Unit Price
                columns.ConstantColumn(80); // Total
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                header.Cell().Element(CellStyle).Text("Description").SemiBold();
                header.Cell().Element(CellStyle).Text("Unit Price").SemiBold();
                header.Cell().Element(CellStyle).Text("Total").SemiBold();

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold())
                                  .PaddingVertical(5)
                                  .BorderBottom(1)
                                  .BorderColor(Colors.Black);
                }
            });

            // Items
            foreach (var item in items)
            {
                var quantity = getQuantity(item);
                var unitPrice = getUnitPrice(item);
                var total = (decimal)quantity * unitPrice;

                table.Cell().Element(CellStyle).Text(quantity.ToString());
                table.Cell().Element(CellStyle).Text(getDescription(item));
                table.Cell().Element(CellStyle).Text($"${unitPrice:F2}");
                table.Cell().Element(CellStyle).Text($"${total:F2}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.PaddingVertical(3)
                                  .BorderBottom(1)
                                  .BorderColor(Colors.Grey.Lighten2);
                }
            }
        });
    }
}