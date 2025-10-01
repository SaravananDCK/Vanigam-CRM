using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditLedgerEntry
    {
        [Inject] private LedgerEntryApiService LedgerEntryApiService { get; set; }
        [Inject] private CustomerApiService CustomerApiService { get; set; }

        private IEnumerable<LedgerAccount> LedgerAccounts { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
                CurrentObject = new() { EntryDate = DateTimeOffset.UtcNow };
            else
                CurrentObject = await LedgerEntryApiService.GetByOid(oid: Oid);

            await LoadLedgerAccounts();
            await InitEditContext();
        }

        private async Task LoadLedgerAccounts()
        {
            try
            {
                var result = await CustomerApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                LedgerAccounts = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadAccountsFailed"] });
            }
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await LedgerEntryApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await LedgerEntryApiService.Update(oid: Oid, CurrentObject);
                    if (result.IsPreconditionFailed())
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
    }
}
