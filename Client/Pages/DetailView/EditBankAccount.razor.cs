using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditBankAccount
    {
        [Inject] private BankAccountApiService BankAccountApiService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new() { AccountType = AccountType.BankAccount };
                IsReadOnlyMode = false;
            }
            else
            {
                CurrentObject = await BankAccountApiService.GetByOid(oid: Oid);
                IsReadOnlyMode = true;
            }
            await InitEditContext();
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
        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await BankAccountApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await BankAccountApiService.Update(oid: Oid, CurrentObject);
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
