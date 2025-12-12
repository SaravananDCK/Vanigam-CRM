using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services.Pdf;

public class QuotePdfService(
    VanigamAccountingDbContext context,
    ICurrentUserService currentUserService,
    ILogger<QuotePdfService> logger) : BasePdfService<Quote>(context, logger, currentUserService)
{
    public override async Task<Quote> GetEntityWithIncludesAsync(Guid entityId)
    {
        return await Context.Quotes
            .Include(q => q.Party)
            .Include(q => q.Job)
            .Include(q => q.VoucherLines)
                .ThenInclude(qi => qi.Item)
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
                col.Item().Row(row =>
                {
                    row.ConstantItem(45).Text("Quote #:").SemiBold();
                    row.AutoItem().Text($"{quote.Number}");
                });

                col.Item().Row(row =>
                {
                    row.ConstantItem(45).Text("Date:").SemiBold();
                    row.AutoItem().Text($"{quote.CreatedAtUtc:MM/dd/yyyy}");
                });

                col.Item().Row(row =>
                {
                    row.ConstantItem(45).Text("Status:").SemiBold();
                    row.AutoItem().Text($"{quote.Status}");
                });

                if (quote.Job != null)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(45).Text("Job:").SemiBold();
                        row.AutoItem().Text($"{quote.Job.Title}");
                    });
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
        if (quote.VoucherLines.Any())
        {
            BuildItemsTable(
                quote.VoucherLines,
                column,
                item => item.Quantity,
                item => item.Item?.Name ?? "Item",
                item => item.UnitPrice
            );
        }

        // GST LEFT + SUBTOTAL/DISCOUNT RIGHT
        column.Item().PaddingTop(20).Row(row =>
        {
            // LEFT SIDE - GST
            row.ConstantItem(200).AlignRight().Column(left =>
            {
                if (quote.CGSTAmount > 0)
                {
                    left.Item().Row(r =>
                    {
                        r.AutoItem().Text("CGST:").FontSize(11);
                        r.AutoItem().Width(100).AlignRight().Text($"₹{quote.CGSTAmount:N2}").FontSize(11);
                    });
                }

                if (quote.SGSTAmount > 0)
                {
                    left.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Text("SGST:").FontSize(11);
                        r.AutoItem().Width(100).AlignRight().Text($"₹{quote.SGSTAmount:N2}").FontSize(11);
                    });
                }

                if (quote.IGSTAmount > 0)
                {
                    left.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Text("IGST:").FontSize(11);
                        r.AutoItem().Width(100).AlignRight().Text($"₹{quote.IGSTAmount:N2}").FontSize(11);
                    });
                }

                if (quote.CessAmount > 0)
                {
                    left.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Text("CESS:").FontSize(11);
                        r.AutoItem().Width(100).AlignRight().Text($"₹{quote.CessAmount:N2}").FontSize(11);
                    });
                }
            });

            // RIGHT SIDE - SUBTOTAL + DISCOUNT
            row.RelativeItem().AlignRight().Column(right =>
            {
                right.Item().Row(r =>
                {
                    r.AutoItem().Width(120).AlignRight().Text("Sub Total:").FontSize(11);
                    r.AutoItem().Width(130).AlignRight().Text($"₹{quote.SubTotal:N2}").FontSize(11);
                });

                if (quote.DiscountAmount > 0)
                {
                    right.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(120).AlignRight().Text("Discount:").FontSize(11);
                        r.AutoItem().Width(130).AlignRight().Text($"-₹{quote.DiscountAmount:N2}").FontSize(11);
                    });
                }
                if (quote.TaxAmount > 0)
                {
                    right.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Width(120).AlignRight().Text("Tax:").FontSize(11);
                        r.AutoItem().Width(130).AlignRight().Text($"₹{quote.TaxAmount:N2}").FontSize(11);
                    });
                }
            });
        });


        // FULL-WIDTH LINE (LEFT + RIGHT)  ✔
        column.Item().PaddingTop(10).PaddingBottom(5).LineHorizontal(1);


        // TOTAL AMOUNT (RIGHT)
        column.Item().AlignRight().Row(r =>
        {
            r.AutoItem().Width(120).Text("Total Amount:")
                .FontSize(14).SemiBold();
            r.AutoItem().Width(100).AlignRight()
                .Text($"₹{quote.TotalAmount:N2}")
                .FontSize(14).SemiBold();
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