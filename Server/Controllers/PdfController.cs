using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PdfController(
    QuotePdfService quotePdfService,
    InvoicePdfService invoicePdfService,
    ILogger<PdfController> logger) : ControllerBase
{
    [HttpGet("quote/{quoteId}")]
    public async Task<ActionResult> GenerateQuotePdf(Guid quoteId)
    {
        try
        {
            logger.LogInformation("Generating PDF for Quote {QuoteId}", quoteId);

            var pdfBytes = await quotePdfService.GeneratePdfAsync(quoteId);

            return File(pdfBytes, "application/pdf", $"Quote_{quoteId:N}.pdf");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Quote {QuoteId} not found: {Message}", quoteId, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating PDF for Quote {QuoteId}", quoteId);
            return StatusCode(500, "An error occurred while generating the PDF");
        }
    }

    [HttpGet("quote/{quoteId}/preview")]
    public async Task<ActionResult> PreviewQuotePdf(Guid quoteId)
    {
        try
        {
            logger.LogInformation("Generating PDF preview for Quote {QuoteId}", quoteId);

            var pdfBytes = await quotePdfService.GeneratePdfAsync(quoteId);

            Response.Headers.Add("Content-Disposition", "inline");
            return File(pdfBytes, "application/pdf");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Quote {QuoteId} not found: {Message}", quoteId, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating PDF preview for Quote {QuoteId}", quoteId);
            return StatusCode(500, "An error occurred while generating the PDF preview");
        }
    }

    [HttpGet("invoice/{invoiceId}")]
    public async Task<ActionResult> GenerateInvoicePdf(Guid invoiceId)
    {
        try
        {
            logger.LogInformation("Generating PDF for Invoice {InvoiceId}", invoiceId);

            var pdfBytes = await invoicePdfService.GeneratePdfAsync(invoiceId);

            return File(pdfBytes, "application/pdf", $"Invoice_{invoiceId:N}.pdf");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invoice {InvoiceId} not found: {Message}", invoiceId, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating PDF for Invoice {InvoiceId}", invoiceId);
            return StatusCode(500, "An error occurred while generating the PDF");
        }
    }

    [HttpGet("invoice/{invoiceId}/preview")]
    public async Task<ActionResult> PreviewInvoicePdf(Guid invoiceId)
    {
        try
        {
            logger.LogInformation("Generating PDF preview for Invoice {InvoiceId}", invoiceId);

            var pdfBytes = await invoicePdfService.GeneratePdfAsync(invoiceId);

            Response.Headers.Add("Content-Disposition", "inline");
            return File(pdfBytes, "application/pdf");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invoice {InvoiceId} not found: {Message}", invoiceId, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating PDF preview for Invoice {InvoiceId}", invoiceId);
            return StatusCode(500, "An error occurred while generating the PDF preview");
        }
    }
}