using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class QuotePdfService(
    VanigamAccountingDbContext context,
    ILogger<QuotePdfService> logger) : BasePdfService<Quote>(context, logger)
{
    public override async Task<Quote?> GetEntityWithIncludesAsync(Guid entityId)
    {
        return await Context.Quotes
            .Include(q => q.Party)
            .Include(q => q.Job)
            .Include(q => q.Items)
                .ThenInclude(qi => qi.InventoryItem)
            .FirstOrDefaultAsync(q => q.Oid == entityId);
    }

    public override string GetDocumentTitle() => "QUOTATION";

    public override Color GetDocumentColor() => Colors.Blue.Medium;

    public override void BuildDocumentHeader(Quote quote, ColumnDescriptor column)
    {
        // Company Info and Quote Details
        column.Item().Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                BuildCompanyInfo(col);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text($"Quote #: {quote.Number}").SemiBold();
                col.Item().Text($"Date: {quote.CreatedAtUtc?.ToString("MM/dd/yyyy") ?? DateTime.Now.ToString("MM/dd/yyyy")}");
                col.Item().Text($"Status: {quote.Status}");
                if (quote.Job != null)
                {
                    col.Item().Text($"Job: {quote.Job.Title}");
                }
            });
        });

        column.Item().PaddingTop(20).LineHorizontal(1);

        // Customer Info
        if (quote.Party != null)
        {
            column.Item().PaddingTop(20).Column(col =>
            {
                BuildCustomerInfo(quote.Party, col);
            });
        }

        column.Item().PaddingTop(20);
    }

    public override void BuildDocumentContent(Quote quote, ColumnDescriptor column)
    {
        // Items Table
        if (quote.Items.Any())
        {
            BuildItemsTable(
                quote.Items,
                column,
                item => item.Quantity,
                item => item.InventoryItem?.Name ?? "Item",
                item => item.UnitPrice
            );
        }

        // Total
        column.Item().PaddingTop(20).AlignRight().Column(col =>
        {
            col.Item().Text($"Total: ${quote.TotalAmount:F2}").FontSize(14).SemiBold();
        });
    }

    public override void BuildDocumentFooter(Quote quote, ColumnDescriptor column)
    {
        // Terms and Conditions
        column.Item().PaddingTop(30).Column(col =>
        {
            col.Item().Text("Terms and Conditions:").SemiBold();
            col.Item().PaddingTop(5).Text("This quote is valid for 30 days from the date above. All work will be performed according to our standard terms and conditions.");
        });
    }
}