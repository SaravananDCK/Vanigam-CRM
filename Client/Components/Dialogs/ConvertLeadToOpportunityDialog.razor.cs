using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.Dialogs;

public partial class ConvertLeadToOpportunityDialog
{
    [Parameter] public Lead Lead { get; set; }
    [Parameter] public EventCallback<Opportunity> OnConverted { get; set; }
    [Parameter] public EventCallback OnCanceled { get; set; }
    private RadzenTemplateForm<ConversionModel> form;
    private ConversionModel model = new();
    private bool isBusy = false;

    protected override void OnParametersSet()
    {
        if (Lead != null)
        {
            model.Title = $"{Lead.Organization ?? Lead.Name} - {Lead.ProductOfInterest ?? "Opportunity"}";
            model.EstimatedValue = Lead.EstimatedBudget ?? 0;
            model.ExpectedCloseDate = DateTime.Today.AddDays(30); // Default to 30 days from now
        }
    }

    private async Task OnSubmit()
    {
        if (Lead == null) return;

        isBusy = true;
        try
        {
            var opportunity = await ConversionService.ConvertLeadToOpportunityAsync(
                Lead.Oid,
                model.Title,
                model.EstimatedValue,
                model.ExpectedCloseDate);

            if (opportunity != null)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = Localizer["Success"],
                    Detail = Localizer["LeadConvertedSuccessfully"]
                });
                DialogService.CloseDialog(Lead);
                await OnConverted.InvokeAsync(opportunity);
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
        public string Title { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
        public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(30);
    }
}