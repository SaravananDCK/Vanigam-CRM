using System.Net;
using Microsoft.AspNetCore.Components;
using Radzen;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditLedgerAccount
    {
        private IEnumerable<AccountGroup> AccountGroups = Enumerable.Empty<AccountGroup>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadAccountGroups();
            if (Oid == Guid.Empty)
            {
                CurrentObject = new()
                {
                    IsActive = true,
                    Balance = 0,
                    CreditLimit = 0
                };
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await LedgerAccountApiService.GetByOid(
                    oid: Oid,
                    expand: "AccountGroup");
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
                    CurrentObject = await LedgerAccountApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await LedgerAccountApiService.Update(oid: Oid, CurrentObject);
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

        private async Task LoadAccountGroups()
        {
            try
            {
                var result = await AccountGroupApiService.Get(
                    filter: "IsNotDeleted eq true and IsActive eq true",
                    orderBy: "Name",
                    top: 1000);

                AccountGroups = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer[$"Error"],
                    Detail = Localizer[$"Load"]
                });
            }
        }
    }
}
