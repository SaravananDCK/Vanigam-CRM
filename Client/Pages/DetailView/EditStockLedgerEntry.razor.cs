using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditStockLedgerEntry
    {
        [Inject] private StockLedgerEntryApiService StockLedgerEntryApiService { get; set; }
        [Inject] private InventoryItemApiService InventoryItemApiService { get; set; }
        [Inject] private LocationApiService LocationApiService { get; set; }

        private IEnumerable<InventoryItem> InventoryItems { get; set; } = [];
        private IEnumerable<Location> Locations { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
                CurrentObject = new() { EntryDate = DateTimeOffset.UtcNow };
            else
                CurrentObject = await StockLedgerEntryApiService.GetByOid(oid: Oid);

            await LoadInventoryItems();
            await LoadLocations();
            await InitEditContext();
        }

        private async Task LoadInventoryItems()
        {
            try
            {
                var result = await InventoryItemApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                InventoryItems = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadItemsFailed"] });
            }
        }

        private async Task LoadLocations()
        {
            try
            {
                var result = await LocationApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                Locations = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadLocationsFailed"] });
            }
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await StockLedgerEntryApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await StockLedgerEntryApiService.Update(oid: Oid, CurrentObject);
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
