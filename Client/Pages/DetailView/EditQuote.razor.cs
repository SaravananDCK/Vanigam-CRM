using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Client.Components.Dialogs;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditQuote
    {
        [Inject] private QuoteApiService QuoteApiService { get; set; }
        private int QuotationFor { get; set; }
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await QuoteApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
            }

            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<Invoice>()
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }
        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await QuoteApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await QuoteApiService.Update(oid: Oid, CurrentObject);
                    if(result.IsPreconditionFailed())
                    {
                        HasChanges = true;
                        CanEdit = false;
                        return;
                    }
                }
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = Localizer["SavedSuccessfully!"] });
                DialogService.CloseDialog(CurrentObject);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    ShowNotUniqueAlert = true;
                }
                else
                {
                    ErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                    ErrorVisible = true;
            }
            IsBusy = false;
        }

        protected override async Task SaveAndStayInEdit()
        {
            await FormSubmit();
            // After successful save, switch back to read-only mode
            if (!ErrorVisible && !ShowNotUniqueAlert)
            {
                IsReadOnlyMode = true;
                StateHasChanged();
            }
        }

        // Business rule validation for conversion
        private bool CanConvertToInvoice => CurrentObject != null &&
            CurrentObject.Status == QuoteStatus.Accepted &&
            IsReadOnlyMode && !IsCreateMode;

        private async Task ShowConversionDialog()
        {
            var result = await DialogService.OpenDialogAsync<ConvertQuoteToInvoiceDialog>(
                Localizer["ConvertToInvoice"],
                new Dictionary<string, object> { { "Quote", CurrentObject } },
                50, 40);

            if (result != null)
            {
                // Refresh quote to show any updated information
                CurrentObject = await QuoteApiService.GetByOid(oid: Oid);
                StateHasChanged();
            }
        }
        private async Task SaveBulkQuote()
        {
            if (CurrentObject == null) return;

            IsBusy = true;
            try
            {
                var bulkData = new QuoteBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    Title = CurrentObject.Number,
                    Status = CurrentObject.Status,
                    OpportunityId = CurrentObject.OpportunityId,
                    CustomerId = CurrentObject.PartyId,
                    JobId = CurrentObject.JobId,
                    TotalAmount = CurrentObject.TotalAmount,
                    Items = quoteItems
                };

                var savedQuote = await QuoteApiService.BulkSaveQuoteWithItemsAsync(bulkData);

                if (savedQuote != null)
                {
                    CurrentObject = savedQuote;
                    hasQuoteItemChanges = false;

                    await LoadQuoteItems(); // Refresh items
                    await EnableReadOnlyModeAsync();

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = Localizer["Success"],
                        Detail = Localizer["QuoteSavedSuccessfully"]
                    });
                    DialogService.CloseDialog(CurrentObject);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = ex.Message
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
