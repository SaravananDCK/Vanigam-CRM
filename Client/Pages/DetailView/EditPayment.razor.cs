using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NodaTime;
using Radzen;
using Radzen.Blazor;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditPayment
    {
        [Inject] private PaymentApiService PaymentApiService { get; set; }
        [Inject] private InvoiceApiService InvoiceApiService { get; set; }
        [Inject] private BankAccountApiService BankAccountApiService { get; set; }
        
        private List<Invoice> PendingInvoices = new();
        private List<PaymentAllocationDTO> Allocations = new();
        private List<BankAccount> BankAccounts = new();
        private RadzenDataGrid<PaymentAllocationDTO> PaymentsGrid = null!;
        private decimal totalAllocated = 0;
        private decimal remainingAmount = 0;
        private static readonly IList<PaymentStatus> PaymentStatuses = [.. Enum.GetValues<PaymentStatus>()];
        private static readonly IList<PaymentMethod> PaymentMethods = [.. Enum.GetValues<PaymentMethod>()];
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                CurrentObject.VoucherDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
                CurrentObject.Status = PaymentStatus.Pending;
                CurrentObject.PaymentMethod = PaymentMethod.Cash;
                CurrentObject.AllocatedAmount = totalAllocated;
                CurrentObject.PaidAt = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await PaymentApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
            }
            // Load bank accounts
            await LoadBankAccounts();
            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<Payment>()
                .Expand(f => f.Applications)
                .Expand(f => f.Party, f => f.Party.Name)
                .Expand(f => f.BankAccount, f => f.BankAccount.AccountNumber)
                .Build();
        }
        private async Task PaymentStatusChanged(PaymentStatus status)
        {
            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
            StateHasChanged();
        }
        private async Task LoadBankAccounts()
        {
            try
            {
                var result = await BankAccountApiService.Get(filter: "IsActive eq true and IsNotDeleted eq true", orderBy: "Name", top: 100);
                BankAccounts = result.Value.ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["ErrorLoadingBankAccounts"]
                });
            }
        }

        private async Task OnCustomerChanged()
        {
            if (CurrentObject.PartyId.HasValue && CurrentObject.PartyId.Value != Guid.Empty)
            {
                await LoadPendingInvoices(CurrentObject.PartyId.Value);
                await CalculatePaymentAmount();
                
            }
            else
            {
                PendingInvoices.Clear();
                Allocations.Clear();
            }
            StateHasChanged();
        }
        private async Task CalculatePaymentAmount()
        {
            if (PendingInvoices.Any())
            {
                CurrentObject.PaymentAmount = Allocations.Where(a => a.InvoiceId != Guid.Empty).Sum(a => a.BalanceAmount);
                await OnPaymentAmountChanged(CurrentObject.PaymentAmount);
            }
        }
        private async Task LoadPendingInvoices(Guid customerId)
        {
            try
            {
                var filter = $"PartyId eq {customerId} and BalanceAmount gt 0 and IsNotDeleted eq true";
                var result = await InvoiceApiService.Get(filter: filter, orderBy: "DueDate", top: 100);
                PendingInvoices = result.Value.ToList();
                
                
                // Initialize allocations
                Allocations = PendingInvoices.Select(inv => new PaymentAllocationDTO
                {
                    InvoiceId = inv.Oid,
                    InvoiceNumber = inv.Number,
                    InvoiceDate = inv.VoucherDate,
                    DueDate = inv.DueDate,
                    TotalAmount = inv.TotalAmount,
                    BalanceAmount = inv.BalanceAmount,
                    AllocatedAmount = 0,
                    IsSelected = false
                }).ToList();

                CalculateTotals();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = ex.Message
                });
            }
        }

        private async Task OnPaymentAmountChanged(decimal amount)
        {
            CurrentObject.PaymentAmount = amount;
            CurrentObject.UnallocatedAmount = amount;
            AutoAllocate();
            //CalculateTotals();
            await PaymentsGrid.Reload();
        }

        private void AutoAllocate()
        {
            if (CurrentObject.PaymentAmount <= 0)
                return;

            var remainingToAllocate = CurrentObject.PaymentAmount;

            // Only allocate to invoice lines (exclude Customer Advance line)
            foreach (var allocation in Allocations.Where(a => a.InvoiceId != Guid.Empty).OrderBy(a => a.DueDate))
            {
                if (remainingToAllocate <= 0)
                {
                    allocation.AllocatedAmount = 0;
                    allocation.IsSelected = false;
                }
                else
                {
                    var allocAmount = Math.Min(remainingToAllocate, allocation.BalanceAmount);
                    allocation.AllocatedAmount = allocAmount;
                    allocation.IsSelected = allocAmount > 0;
                    remainingToAllocate -= allocAmount;
                }
            }


            // Remove existing Customer Advance line if present
            var existingAdvanceLine = Allocations.FirstOrDefault(a => a.InvoiceId == Guid.Empty);
            if (existingAdvanceLine != null)
            {
                Allocations.Remove(existingAdvanceLine);
            }
            // If there's excess payment (overpayment), add Customer Advance line
            if (remainingToAllocate > 0)
            {
                // Add new Customer Advance line for excess amount
                Allocations.Add(new PaymentAllocationDTO
                {
                    InvoiceId = Guid.Empty, // Empty GUID indicates this is a Customer Advance
                    InvoiceNumber = "Customer Advance",
                    InvoiceDate = CurrentObject.VoucherDate,
                    DueDate = null,
                    TotalAmount = remainingToAllocate,
                    BalanceAmount = remainingToAllocate,
                    AllocatedAmount = remainingToAllocate,
                    Amount = remainingToAllocate,
                    IsSelected = true
                });
            }

            CalculateTotals();
            StateHasChanged();
        }

        private void OnAllocationChanged(PaymentAllocationDTO allocation)
        {
            // Don't allow manual changes to Customer Advance line - it auto-calculates
            if (allocation.InvoiceId == Guid.Empty)
            {
                return;
            }

            if (allocation.AllocatedAmount > allocation.BalanceAmount)
            {
                allocation.AllocatedAmount = allocation.BalanceAmount;
            }

            if (allocation.AllocatedAmount < 0)
            {
                allocation.AllocatedAmount = 0;
            }

            allocation.IsSelected = allocation.AllocatedAmount > 0;

            // Remove Customer Advance line if it exists (user is manually allocating)
            var existingAdvanceLine = Allocations.FirstOrDefault(a => a.InvoiceId == Guid.Empty);
            if (existingAdvanceLine != null)
            {
                Allocations.Remove(existingAdvanceLine);
            }

            CalculateTotals();
        }

        private void CalculateTotals()
        {
            totalAllocated = Allocations.Where(a => a.InvoiceId != Guid.Empty).Sum(a => a.AllocatedAmount);
            remainingAmount = CurrentObject.PaymentAmount - totalAllocated;
            CurrentObject.AllocatedAmount = totalAllocated;
            CurrentObject.UnallocatedAmount = remainingAmount;
        }

        private async Task SaveBulkPayment()
        {
            if (CurrentObject == null) return;

            IsBusy = true;
            try
            {
                // Prepare bulk save DTO with payment and allocations
                var bulkData = new PaymentBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    PartyId = CurrentObject.PartyId,
                    PaymentAmount = CurrentObject.PaymentAmount,
                    VoucherDate = CurrentObject.VoucherDate,
                    PaidAt = CurrentObject.PaidAt,
                    PaymentMethod = CurrentObject.PaymentMethod,
                    ReferenceNumber = CurrentObject.ReferenceNumber,
                    BankAccountId = CurrentObject.BankAccountId,
                    Status = CurrentObject.Status,
                    AllocatedAmount = totalAllocated,
                    UnallocatedAmount = remainingAmount,
                    Allocations = Allocations
                        .Where(a => a.IsSelected && a.AllocatedAmount > 0 && a.InvoiceId != Guid.Empty) // Exclude Customer Advance line
                        .Select(a => new PaymentAllocationDTO
                        {
                            InvoiceId = a.InvoiceId,
                            Amount = a.AllocatedAmount,
                            AllocatedAmount = a.AllocatedAmount,
                            InvoiceNumber = a.InvoiceNumber
                        }).ToList()
                };

                var savedPayment = await PaymentApiService.BulkSavePaymentWithAllocationsAsync(bulkData);

                if (savedPayment != null)
                {
                    CurrentObject = savedPayment;

                    if (IsCreateMode)
                    {
                        // Navigate to edit mode for the newly created payment
                        NavigationManager.NavigateTo($"/edit-payment?oid={savedPayment.Oid}");
                    }
                    else
                    {
                        await EnableReadOnlyModeAsync();
                    }

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = Localizer["Success"],
                        Detail = Allocations.Any(a => a.IsSelected)
                            ? Localizer[$"Payment saved with {Allocations.Count(a => a.IsSelected)} allocations"]
                            : Localizer["PaymentSavedSuccessfully"]
                    });
                }
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
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
                Logger.LogError(ex.Message, ex);
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

        protected override async Task EnableReadOnlyModeAsync()
        {
            await base.EnableReadOnlyModeAsync();
            if (CurrentObject?.PartyId.HasValue == true)
            {
                await LoadPendingInvoices(CurrentObject.PartyId.Value);
            }
        }

        /// <summary>
        /// Determines if the selected payment method requires a bank account.
        /// </summary>
        private bool RequiresBankAccount()
        {
            if (CurrentObject == null)
                return false;

            return CurrentObject.PaymentMethod switch
            {
                Objects.Entities.PaymentMethod.BankTransfer => true,
                Objects.Entities.PaymentMethod.Cheque => true,
                Objects.Entities.PaymentMethod.Card => true,
                Objects.Entities.PaymentMethod.UPI => true,
                Objects.Entities.PaymentMethod.NetBanking => true,
                _ => false // Cash, Wallet, Other don't require bank account
            };
        }

        /// <summary>
        /// Handles payment method changes. Clears bank account if cash/wallet selected.
        /// </summary>
        private async Task PaymentMethodChanged(PaymentMethod value)
        {
            OnPaymentMethodChanged(value);
        }

        private void OnPaymentMethodChanged(object value)
        {
            if (!RequiresBankAccount())
            {
                // Clear bank account for cash/wallet/other payments
                CurrentObject.BankAccountId = null;
            }
            StateHasChanged();
        }

    }
}
