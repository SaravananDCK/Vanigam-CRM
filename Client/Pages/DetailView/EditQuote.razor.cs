using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Client.Components.Dialogs;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditQuote
    {
        [Inject] private QuoteApiService QuoteApiService { get; set; }
        [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }
        [Inject] private CustomerApiService CustomerApiService { get; set; }
        [Inject] private OpportunityApiService OpportunityApiService { get; set; }
        private IEnumerable<Customer> Customers { get; set; } = [];
        private IEnumerable<Opportunity> Opportunities { get; set; }
        private int QuotationFor { get; set; }
        public string TenantAccountingState { get; set; }
        private string CurrentState { get; set; }
        private List<QuoteItemDTO> quoteItems = new();
        private bool hasItemChanges = false;
        public bool HasAnyChanges => Form?.EditContext?.IsModified() == true || hasItemChanges || (quoteItems?.Any(i => i.IsNew || i.IsDeleted) ?? false);
        private static readonly IList<QuoteStatus> QuoteStatuses = [.. Enum.GetValues<QuoteStatus>()];
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (CurrentObject != null && !IsCreateMode)
            {
                await LoadQuoteItems();
            }
            else if (IsCreateMode)
            {
                quoteItems = new List<QuoteItemDTO>();
            }
        }

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
            var result = await TenantAccountingSettingsApiService.Get(top: 1);
            var accSetings = result?.Value?.FirstOrDefault(f => !string.IsNullOrEmpty(f.CompanyState));
            TenantAccountingState = accSetings?.CompanyState;
            await LoadDropdownData();
            await InitEditContext();
        }
        private async Task Changed(QuoteStatus status)
        {

            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
            StateHasChanged();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<Quote>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }
        private async Task LoadDropdownData()
        {
            try
            {
                var opportunitiesTask = OpportunityApiService.Get();
                var customersTask = CustomerApiService.Get();

                await Task.WhenAll(opportunitiesTask, customersTask);

                Opportunities = (await opportunitiesTask).Value;
                Customers = (await customersTask).Value;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["FailedToLoadDropdownData"]
                });
            }
        }

        private async Task LoadQuoteItems()
        {
            try
            {
                if (Oid != Guid.Empty)
                {
                    quoteItems = await QuoteApiService.GetQuoteItemsForEditingAsync(Oid);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["FailedToLoadQuoteItems"]
                });
            }
        }

        private async Task OnQuoteItemsChanged(List<QuoteItemDTO> items)
        {
            if(QuotationFor == 0)
            {
                CurrentState = Customers.FirstOrDefault(c => c.Oid == CurrentObject.PartyId)?.State;
            }else if(QuotationFor == 1)
            {
                CurrentState = Opportunities.FirstOrDefault(o => o.Oid == CurrentObject.OpportunityId)?.Lead.State;
            }
            
            if (items.Any(i => i.InventoryItemId == null)) return;
            quoteItems = items;
            hasItemChanges = true;
            CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            var subTotal = quoteItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
            var totalDiscount = quoteItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
            var totalTax = quoteItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

            // Calculate GST breakdown from purchase invoice items
            decimal cgstAmount = 0;
            decimal sgstAmount = 0;
            decimal igstAmount = 0;
            decimal cessAmount = 0;

            foreach (var item in quoteItems.Where(i => !i.IsDeleted))
            {
                var taxableAmount = item.Total - item.DiscountAmount;
                if (TenantAccountingState == CurrentState)
                {
                    cgstAmount += taxableAmount * (decimal)(item.CGSTRate / 100);
                    sgstAmount += taxableAmount * (decimal)(item.SGSTRate / 100);
                }
                else
                {
                    igstAmount += taxableAmount * (decimal)(item.IGSTRate / 100);
                }
                cessAmount += taxableAmount * (decimal)(item.CessRate / 100);
            }

            CurrentObject.SubTotal = Math.Round(subTotal);
            CurrentObject.DiscountAmount = totalDiscount;
            CurrentObject.TaxAmount = Math.Round(totalTax);
            CurrentObject.CGSTAmount = cgstAmount;
            CurrentObject.SGSTAmount = sgstAmount;
            CurrentObject.IGSTAmount = igstAmount;
            CurrentObject.CessAmount = cessAmount;
            CurrentObject.TotalAmount = Math.Round(subTotal - totalDiscount + totalTax);

            StateHasChanged();
        }
        private async Task OnDiscountTypeChanged(DiscountType type)
        {
            CurrentObject.DiscountType = type;
            StateHasChanged();
        }
        private async Task OnDiscountPercentageChanged(decimal discountPercent)
        {
            if (CurrentObject != null && discountPercent != 0)
            {
                CurrentObject.DiscountPercent = discountPercent;
                EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.DiscountPercent)));
                StateHasChanged();
            }
        }

        protected override async Task EnableReadOnlyModeAsync()
        {
            await base.EnableReadOnlyModeAsync();
            hasItemChanges = false;
            await LoadQuoteItems(); // Reload original data
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
                DialogService.CloseDialog(CurrentObject);
            }
        }
        protected async Task FormSubmit()
        {
            if (quoteItems.Any() && quoteItems.FirstOrDefault().InventoryItemId != null)
            {
                await SaveBulkQuote();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Failed"],
                    Detail = Localizer["At least one Quote Item is required.."]
                });
            }
        }
        private async Task SaveBulkQuote()
        {
            if (CurrentObject == null) return;
            
            var isValid = EditContext.Validate();
            if (!isValid) return;

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
                    SubTotal = CurrentObject.SubTotal,
                    TaxAmount = CurrentObject.TaxAmount,
                    CGSTAmount = CurrentObject.CGSTAmount,
                    SGSTAmount = CurrentObject.SGSTAmount,
                    IGSTAmount = CurrentObject.IGSTAmount,
                    CessAmount = CurrentObject.CessAmount,
                    DiscountType = CurrentObject.DiscountType,
                    DiscountAmount = CurrentObject.DiscountAmount,
                    DiscountPercentage = CurrentObject.DiscountPercent,
                    Items = quoteItems
                };

                var savedQuote = await QuoteApiService.BulkSaveQuoteWithItemsAsync(bulkData);

                if (savedQuote != null)
                {
                    CurrentObject = savedQuote;
                    hasItemChanges = false;

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
    }
}
