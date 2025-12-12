using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Radzen.Blazor;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services.Pdf;

public class InvoicePdfService(
    VanigamAccountingDbContext context,
    ICurrentUserService currentUserService,
    ILogger<InvoicePdfService> logger) : BasePdfService<Invoice>(context, logger, currentUserService)
{
    public override async Task<Invoice> GetEntityWithIncludesAsync(Guid entityId)
    {
        return await Context.Invoices
            .Include(i => i.Party)
            .Include(i => i.VoucherLines)
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
                col.Item().Row(row =>
                {
                    row.ConstantItem(50).Text("Invoice #:").SemiBold();
                    row.AutoItem().Text($"{invoice.Number}");
                });
                col.Item().Row(row =>
                {
                    row.ConstantItem(50).Text("Date:").SemiBold();
                    row.AutoItem().Text($"{invoice.CreatedAtUtc?.ToString("MM/dd/yyyy") ?? DateTime.Now.ToString("MM/dd/yyyy")}");
                });
                col.Item().Row(row =>
                {
                    row.ConstantItem(50).Text("Status:").SemiBold();
                    row.AutoItem().Text($"{invoice.Status.GetDisplayDescription()}");
                });

                if (invoice.Party != null)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(50).Text("Customer:").SemiBold();
                        row.AutoItem().Text($"{invoice.Party.Name}");
                    });
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
        if (invoice.VoucherLines.OfType<InvoiceItem>().Any())
        {
            BuildItemsTable(
                invoice.VoucherLines.OfType<InvoiceItem>(),
                column,
                item => item.Quantity,
                item => item.Item?.Name ?? "Item",
                item => item.UnitPrice
            );
        }

        // Totals Section
        column.Item().PaddingTop(20).AlignRight().Column(col =>
        {
            column.Item().PaddingTop(20).Row(row =>
            {
                // LEFT SIDE - GST
                row.ConstantItem(200).AlignRight().Column(left =>
                {
                    if (invoice.CGSTAmount > 0)
                    {
                        left.Item().Row(r =>
                        {
                            r.AutoItem().Text("CGST:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${invoice.CGSTAmount:N2}").FontSize(11);
                        });
                    }

                    if (invoice.SGSTAmount > 0)
                    {
                        left.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("SGST:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${invoice.SGSTAmount:N2}").FontSize(11);
                        });
                    }

                    if (invoice.IGSTAmount > 0)
                    {
                        left.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("IGST:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${invoice.IGSTAmount:N2}").FontSize(11);
                        });
                    }

                    if (invoice.CessAmount > 0)
                    {
                        left.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("CESS:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${invoice.CessAmount:N2}").FontSize(11);
                        });
                    }
                });

                // RIGHT SIDE - SUBTOTAL + DISCOUNT
                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Row(r =>
                    {
                        r.AutoItem().Width(120).AlignRight().Text("Sub Total:").FontSize(11);
                        r.AutoItem().Width(130).AlignRight().Text($"${invoice.SubTotal:N2}").FontSize(11);
                    });

                    if (invoice.DiscountAmount > 0)
                    {
                        right.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Width(120).AlignRight().Text("Discount:").FontSize(11);
                            r.AutoItem().Width(130).AlignRight().Text($"-${invoice.DiscountAmount:N2}").FontSize(11);
                        });
                    }
                    if (invoice.TaxAmount > 0)
                    {
                        right.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Width(120).AlignRight().Text("Tax:").FontSize(11);
                            r.AutoItem().Width(130).AlignRight().Text($"${invoice.TaxAmount:N2}").FontSize(11);
                        });
                    }
                });
            });


            // FULL-WIDTH LINE (LEFT + RIGHT)  ✔
            column.Item().PaddingTop(10).PaddingBottom(5).LineHorizontal(1);


            // TOTAL AMOUNT (RIGHT)
            column.Item().AlignRight().Row(r =>
            {
                r.AutoItem().Width(120).AlignRight().Text("Total Amount:")
                    .FontSize(14).SemiBold();
                r.AutoItem().Width(130).AlignRight()
                    .Text($"${invoice.TotalAmount:N2}")
                    .FontSize(14).SemiBold();
            });

            // Show payments if any
            if (invoice.Allocations.Any())
            {
                var totalPaid = invoice.Allocations.Sum(p => p.Amount);
                var balance = invoice.TotalAmount - totalPaid;
                column.Item().PaddingTop(10).AlignRight().Row(r =>
                {
                    r.AutoItem().Width(120).AlignRight().Text("Payments:").FontSize(14).SemiBold().FontColor(Colors.Green.Medium);
                    r.AutoItem().Width(130).AlignRight().Text($"${totalPaid:F2}").FontSize(14).SemiBold().FontColor(Colors.Green.Medium);
                });
                column.Item().PaddingTop(10).AlignRight().Row(r =>
                {
                    r.AutoItem().Width(120).AlignRight().Text("Balance Due:").FontSize(14).SemiBold().FontColor(balance > 0 ? Colors.Red.Medium : Colors.Green.Medium); 
                    r.AutoItem().Width(130).AlignRight().Text($"${balance:F2}").FontSize(14).SemiBold().FontColor(balance > 0 ? Colors.Red.Medium : Colors.Green.Medium); 
                });
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