using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Client.Components.Dialogs;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditQuote
    {
        [Inject] private QuoteApiService QuoteApiService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await QuoteApiService.GetByOid(oid: Oid);
                IsReadOnlyMode = true; // Edit mode - start in read-only
            }

            await InitEditContext();
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
    }
}
