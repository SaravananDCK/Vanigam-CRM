using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Radzen.Blazor;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services.Pdf;

public class PurchaseInvoicePdfService(
    VanigamAccountingDbContext context,
    ICurrentUserService currentUserService,
    ILogger<PurchaseInvoicePdfService> logger) : BasePdfService<PurchaseInvoice>(context, logger, currentUserService)
{
    public override async Task<PurchaseInvoice> GetEntityWithIncludesAsync(Guid entityId)
    {
        return await Context.PurchaseInvoices
            .Include(i => i.Party)
            .Include(i => i.VoucherLines)
                .ThenInclude(ii => ii.Item)
            .FirstOrDefaultAsync(i => i.Oid == entityId);
    }

    public override string GetDocumentTitle() => "Purchase Invoice";

    public override Color GetDocumentColor() => Colors.Red.Medium;

    public override void BuildDocumentHeader(PurchaseInvoice purchaseInvoice, ColumnDescriptor column)
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
                    row.AutoItem().Text($"{purchaseInvoice.Number}");
                });
                col.Item().Row(row =>
                {
                    row.ConstantItem(50).Text("Date:").SemiBold();
                    row.AutoItem().Text($"{purchaseInvoice.CreatedAtUtc?.ToString("MM/dd/yyyy") ?? DateTime.Now.ToString("MM/dd/yyyy")}");
                });
                col.Item().Row(row =>
                {
                    row.ConstantItem(50).Text("Status:").SemiBold();
                    row.AutoItem().Text($"{purchaseInvoice.Status.GetDisplayDescription()}");
                });

                if (purchaseInvoice.Party != null)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(50).Text("Vendor:").SemiBold();
                        row.AutoItem().Text($"{purchaseInvoice.Party.Name}");
                    });
                }
            });
        });

        column.Item().PaddingTop(20).LineHorizontal(1);

        // Customer Info
        if (purchaseInvoice.Party != null)
        {
            column.Item().PaddingTop(20).Column(col =>
            {
                BuildCustomerInfo(purchaseInvoice.Party, col);
            });
        }

        column.Item().PaddingTop(20);
    }

    public override void BuildDocumentContent(PurchaseInvoice purchaseInvoice, ColumnDescriptor column)
    {
        // Items Table
        if (purchaseInvoice.VoucherLines.OfType<PurchaseInvoiceItem>().Any())
        {
            BuildItemsTable(
                purchaseInvoice.VoucherLines.OfType<PurchaseInvoiceItem>(),
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
                    if (purchaseInvoice.CGSTAmount > 0)
                    {
                        left.Item().Row(r =>
                        {
                            r.AutoItem().Text("CGST:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${purchaseInvoice.CGSTAmount:N2}").FontSize(11);
                        });
                    }

                    if (purchaseInvoice.SGSTAmount > 0)
                    {
                        left.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("SGST:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${purchaseInvoice.SGSTAmount:N2}").FontSize(11);
                        });
                    }

                    if (purchaseInvoice.IGSTAmount > 0)
                    {
                        left.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("IGST:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${purchaseInvoice.IGSTAmount:N2}").FontSize(11);
                        });
                    }

                    if (purchaseInvoice.CessAmount > 0)
                    {
                        left.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("CESS:").FontSize(11);
                            r.AutoItem().Width(100).AlignRight().Text($"${purchaseInvoice.CessAmount:N2}").FontSize(11);
                        });
                    }
                });

                // RIGHT SIDE - SUBTOTAL + DISCOUNT
                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Row(r =>
                    {
                        r.AutoItem().Width(120).AlignRight().Text("Sub Total:").FontSize(11);
                        r.AutoItem().Width(130).AlignRight().Text($"${purchaseInvoice.SubTotal:N2}").FontSize(11);
                    });

                    if (purchaseInvoice.DiscountAmount > 0)
                    {
                        right.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Width(120).AlignRight().Text("Discount:").FontSize(11);
                            r.AutoItem().Width(130).AlignRight().Text($"-${purchaseInvoice.DiscountAmount:N2}").FontSize(11);
                        });
                    }
                    if (purchaseInvoice.TaxAmount > 0)
                    {
                        right.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Width(120).AlignRight().Text("Tax:").FontSize(11);
                            r.AutoItem().Width(130).AlignRight().Text($"${purchaseInvoice.TaxAmount:N2}").FontSize(11);
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
                    .Text($"${purchaseInvoice.TotalAmount:N2}")
                    .FontSize(14).SemiBold();
            });
        });
    }

    public override void BuildDocumentFooter(PurchaseInvoice purchaseInvoice, ColumnDescriptor column)
    {
        //Terms
        //column.Item().PaddingTop(30).Column(col =>
        //{
        //    col.Item().Text("Payment Terms:").SemiBold();
        //    col.Item().PaddingTop(5).Text("Payment is due within 30 days of Purchase Invoice date. Late payments may incur additional charges.");
        //});
    }
}