using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;
using NodaTime;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditPayment
    {
        [Inject] private PaymentApiService PaymentApiService { get; set; }
        [Inject] private InvoiceApiService InvoiceApiService { get; set; }
        [Inject] private CustomerAdvanceApiService CustomerAdvanceApiService { get; set; }
        [Inject] private PaymentAllocationApiService PaymentAllocationApiService { get; set; }

        private List<Invoice> pendingInvoices = new();
        private List<PaymentAllocationModel> allocations = new();
        private decimal totalAllocated = 0;
        private decimal remainingAmount = 0;

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                CurrentObject.PaymentDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
                CurrentObject.Status = Objects.Entities.PaymentStatus.Pending;
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await PaymentApiService.GetByOid(oid: Oid, expand: "Customer,Applications");
                IsReadOnlyMode = true; // Edit mode - start in read-only
            }

            await InitEditContext();
        }

        private async Task OnCustomerChanged()
        {
            if (CurrentObject.CustomerId.HasValue && CurrentObject.CustomerId.Value != Guid.Empty)
            {
                await LoadPendingInvoices(CurrentObject.CustomerId.Value);
            }
            else
            {
                pendingInvoices.Clear();
                allocations.Clear();
            }
            StateHasChanged();
        }

        private async Task LoadPendingInvoices(Guid customerId)
        {
            try
            {
                var filter = $"PartyId eq {customerId} and BalanceAmount gt 0 and IsNotDeleted eq true";
                var result = await InvoiceApiService.Get(filter: filter, orderBy: "DueDate", top: 100);
                pendingInvoices = result.Value.ToList();

                // Initialize allocations
                allocations = pendingInvoices.Select(inv => new PaymentAllocationModel
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

        private void OnPaymentAmountChanged(decimal amount)
        {
            CurrentObject.PaymentAmount = amount;
            CurrentObject.UnallocatedAmount = amount;
            AutoAllocate();
            CalculateTotals();
        }

        private void AutoAllocate()
        {
            if (CurrentObject.PaymentAmount <= 0)
                return;

            var remainingToAllocate = CurrentObject.PaymentAmount;

            foreach (var allocation in allocations.OrderBy(a => a.DueDate))
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

            CalculateTotals();
            StateHasChanged();
        }

        private void OnAllocationChanged(PaymentAllocationModel allocation)
        {
            if (allocation.AllocatedAmount > allocation.BalanceAmount)
            {
                allocation.AllocatedAmount = allocation.BalanceAmount;
            }

            if (allocation.AllocatedAmount < 0)
            {
                allocation.AllocatedAmount = 0;
            }

            allocation.IsSelected = allocation.AllocatedAmount > 0;
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            totalAllocated = allocations.Sum(a => a.AllocatedAmount);
            remainingAmount = CurrentObject.PaymentAmount - totalAllocated;
            CurrentObject.AllocatedAmount = totalAllocated;
            CurrentObject.UnallocatedAmount = remainingAmount;
        }

        public class PaymentAllocationModel
        {
            public Guid InvoiceId { get; set; }
            public string InvoiceNumber { get; set; } = string.Empty;
            public DateTimeOffset InvoiceDate { get; set; }
            public DateTimeOffset? DueDate { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal BalanceAmount { get; set; }
            public decimal AllocatedAmount { get; set; }
            public bool IsSelected { get; set; }
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    // Create new payment
                    CurrentObject = await PaymentApiService.Create(CurrentObject);

                    // Create allocations if any
                    var selectedAllocations = allocations.Where(a => a.IsSelected && a.AllocatedAmount > 0).ToList();
                    if (selectedAllocations.Any())
                    {
                        // Create each payment allocation
                        foreach (var alloc in selectedAllocations)
                        {
                            var paymentAllocation = new PaymentAllocation
                            {
                                Oid = Guid.NewGuid(),
                                PaymentId = CurrentObject.Oid,
                                InvoiceId = alloc.InvoiceId,
                                Amount = alloc.AllocatedAmount,
                                AppliedDate = CurrentObject.PaymentDate,
                                InvoiceBalanceBefore = alloc.BalanceAmount,
                                InvoiceBalanceAfter = alloc.BalanceAmount - alloc.AllocatedAmount,
                                Notes = $"Payment allocation for {alloc.InvoiceNumber}",
                                TenantId = CurrentObject.TenantId,
                                CreatedByUserId = CurrentObject.CreatedByUserId,
                                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                                UpdatedByUserId = CurrentObject.UpdatedByUserId,
                                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                                IsNotDeleted = true
                            };

                            await PaymentAllocationApiService.Create(paymentAllocation);
                        }

                        // Update payment allocated amount
                        CurrentObject.AllocatedAmount = totalAllocated;
                        CurrentObject.UnallocatedAmount = CurrentObject.PaymentAmount - totalAllocated;
                        var result = await PaymentApiService.Update(oid: CurrentObject.Oid, CurrentObject);
                        if (result.IsPreconditionFailed())
                        {
                            HasChanges = true;
                            CanEdit = false;
                            ErrorVisible = true;
                            NotificationService.Notify(new NotificationMessage
                            {
                                Severity = NotificationSeverity.Error,
                                Summary = Localizer["Error"],
                                Detail = Localizer["ConcurrencyError"]
                            });
                            return;
                        }

                        // Update invoices with new balances
                        foreach (var alloc in selectedAllocations)
                        {
                            try
                            {
                                var invoice = await InvoiceApiService.GetByOid(oid: alloc.InvoiceId);
                                if (invoice != null)
                                {
                                    invoice.PaidAmount += alloc.AllocatedAmount;
                                    invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

                                    // Update status
                                    if (invoice.BalanceAmount <= 0)
                                    {
                                        invoice.Status = InvoiceStatus.Paid;
                                        invoice.BalanceAmount = 0;
                                    }
                                    else if (invoice.PaidAmount > 0)
                                    {
                                        invoice.Status = InvoiceStatus.PartiallyPaid;
                                    }

                                    await InvoiceApiService.Update(oid: invoice.Oid, invoice);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"Error updating invoice {alloc.InvoiceNumber}: {ex.Message}", ex);
                            }
                        }
                    }

                    // Create customer advance for unallocated amount if any
                    if (CurrentObject.UnallocatedAmount > 0)
                    {
                        var advance = new CustomerAdvance
                        {
                            Oid = Guid.NewGuid(),
                            PaymentId = CurrentObject.Oid,
                            Amount = CurrentObject.UnallocatedAmount,
                            BalanceAmount = CurrentObject.UnallocatedAmount,
                            AppliedDate = CurrentObject.PaymentDate,
                            Reason = "Overpayment / Advance Payment",
                            IsAvailableForAllocation = true,
                            TenantId = CurrentObject.TenantId,
                            CreatedByUserId = CurrentObject.CreatedByUserId,
                            CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                            UpdatedByUserId = CurrentObject.UpdatedByUserId,
                            UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                            IsNotDeleted = true
                        };

                        // Save the customer advance separately
                        await CustomerAdvanceApiService.Create(advance);

                        // Update payment to reflect advance as allocated
                        CurrentObject.AllocatedAmount += CurrentObject.UnallocatedAmount;
                        CurrentObject.UnallocatedAmount = 0;
                        await PaymentApiService.Update(oid: CurrentObject.Oid, CurrentObject);
                    }
                }
                else
                {
                    var result = await PaymentApiService.Update(oid: Oid, CurrentObject);
                    if (result.IsPreconditionFailed())
                    {
                        HasChanges = true;
                        CanEdit = false;
                        return;
                    }
                }

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = Localizer["SavedSuccessfully!"],
                    Detail = allocations.Any()
                        ? Localizer[$"Payment saved with {allocations.Count(a => a.IsSelected)} allocations"]
                        : Localizer["PaymentSaved"]
                });
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
                Logger.LogError(ex.Message, ex);
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

    }
}
