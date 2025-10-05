using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class InvoicePdfService(
    VanigamAccountingDbContext context,
    ILogger<InvoicePdfService> logger) : BasePdfService<Invoice>(context, logger)
{
    public override async Task<Invoice?> GetEntityWithIncludesAsync(Guid entityId)
    {
        return await Context.Invoices
            .Include(i => i.Party)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Item)
            .Include(i => i.Allocations)
            .FirstOrDefaultAsync(i => i.Oid == entityId);
    }

    public override string GetDocumentTitle() => "INVOICE";

    public override Color GetDocumentColor() => Colors.Red.Medium;

    public override void BuildDocumentHeader(Invoice invoice, ColumnDescriptor column)
    {
        // Company Info and Invoice Details
        column.Item().Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                BuildCompanyInfo(col);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text($"Invoice #: {invoice.Number}").SemiBold();
                col.Item().Text($"Date: {invoice.CreatedAtUtc?.ToString("MM/dd/yyyy") ?? DateTime.Now.ToString("MM/dd/yyyy")}");
                col.Item().Text($"Status: {invoice.Status}");
                if (invoice.Party != null)
                {
                    col.Item().Text($"Customer: {invoice.Party.Name}");
                }
            });
        });

        column.Item().PaddingTop(20).LineHorizontal(1);

        // Customer Info
        if (invoice.Party != null)
        {
            column.Item().PaddingTop(20).Column(col =>
            {
                BuildCustomerInfo(invoice.Party, col);
            });
        }

        column.Item().PaddingTop(20);
    }

    public override void BuildDocumentContent(Invoice invoice, ColumnDescriptor column)
    {
        // Items Table
        if (invoice.Items.Any())
        {
            BuildItemsTable(
                invoice.Items,
                column,
                item => item.Quantity,
                item => item.Item?.Name ?? "Item",
                item => item.UnitPrice
            );
        }

        // Totals Section
        column.Item().PaddingTop(20).AlignRight().Column(col =>
        {
            col.Item().Text($"Subtotal: ${invoice.TotalAmount:F2}");
            // Add tax calculation here if needed
            col.Item().Text($"Total: ${invoice.TotalAmount:F2}").FontSize(14).SemiBold();

            // Show payments if any
            if (invoice.Allocations.Any())
            {
                var totalPaid = invoice.Allocations.Sum(p => p.Amount);
                var balance = invoice.TotalAmount - totalPaid;
                col.Item().PaddingTop(10).Text($"Payments: ${totalPaid:F2}");
                col.Item().Text($"Balance Due: ${balance:F2}").FontSize(14).SemiBold()
                    .FontColor(balance > 0 ? Colors.Red.Medium : Colors.Green.Medium);
            }
        });
    }

    public override void BuildDocumentFooter(Invoice invoice, ColumnDescriptor column)
    {
        // Payment Terms
        column.Item().PaddingTop(30).Column(col =>
        {
            col.Item().Text("Payment Terms:").SemiBold();
            col.Item().PaddingTop(5).Text("Payment is due within 30 days of invoice date. Late payments may incur additional charges.");
        });
    }
}