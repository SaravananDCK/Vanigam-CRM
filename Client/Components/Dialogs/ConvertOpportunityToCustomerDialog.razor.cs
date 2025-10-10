using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.Dialogs;

public partial class ConvertOpportunityToCustomerDialog
{
    [Parameter] public Opportunity Opportunity { get; set; }
    [Parameter] public EventCallback<Customer> OnConverted { get; set; }
    [Parameter] public EventCallback OnCanceled { get; set; }
    private RadzenTemplateForm<CustomerConversionModel> form;
    private CustomerConversionModel model = new();
    private bool isBusy = false;
    private List<string> customerTypes = new() { "Individual", "Company" };

    protected override void OnParametersSet()
    {
        if (Opportunity != null)
        {
            model.CustomerName = Opportunity.Title ?? "New Customer";
            model.CustomerType = "Company"; // Default to Company
        }
    }

    private async Task OnConvert()
    {
        if (Opportunity == null) return;

        isBusy = true;
        try
        {
            var customer = await ConversionService.ConvertOpportunityToCustomerAsync(Opportunity.Oid);

            if (customer != null)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = Localizer["Success"],
                    Detail = Localizer["OpportunityConvertedSuccessfully"]
                });
                DialogService.CloseDialog(Opportunity);
                await OnConverted.InvokeAsync(customer);
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

    private class CustomerConversionModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = "Company";
    }
}