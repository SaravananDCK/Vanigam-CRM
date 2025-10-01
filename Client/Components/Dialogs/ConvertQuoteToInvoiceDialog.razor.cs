using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.Dialogs;

public partial class ConvertQuoteToInvoiceDialog
{
    [Parameter] public Quote Quote { get; set; }
    [Parameter] public EventCallback<Invoice> OnConverted { get; set; }
    [Parameter] public EventCallback OnCanceled { get; set; }
    private RadzenTemplateForm<ConversionModel> form;
    private ConversionModel model = new();
    private bool isBusy = false;

    protected override void OnParametersSet()
    {
        if (Quote != null)
        {
            // Auto-generate a suggested invoice number based on quote number
            var invoiceCount = new Random().Next(1000, 9999); // In real implementation, this would come from API
            model.InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{invoiceCount:D4}";
        }
    }

    private async Task OnSubmit()
    {
        if (Quote == null) return;

        isBusy = true;
        try
        {
            var invoice = await ConversionService.ConvertQuoteToInvoiceAsync(
                Quote.Oid,
                string.IsNullOrWhiteSpace(model.InvoiceNumber) ? null : model.InvoiceNumber);

            if (invoice != null)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = Localizer["Success"],
                    Detail = Localizer["QuoteConvertedSuccessfully"]
                });
                DialogService.CloseDialog();
                await OnConverted.InvokeAsync(invoice);
            }
        }
        catch (InvalidOperationException ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = Localizer["Warning"],
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Error"],
                Detail = Localizer["ConversionFailed"]
            });
        }
        finally
        {
            isBusy = false;
        }
    }

    private async Task Cancel()
    {
        DialogService.CloseDialog();
        await OnCanceled.InvokeAsync();
    }

    private class ConversionModel
    {
        public string? InvoiceNumber { get; set; }
    }
}