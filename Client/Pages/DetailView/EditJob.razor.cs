using DevExpress.Pdf.Native;
using DevExpress.Pdf.Native.BouncyCastle.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditJob
    {
        [Inject] private JobApiService JobApiService { get; set; }
        [Inject] private CustomerApiService CustomerApiService { get; set; }
        [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }
        [Parameter] public Guid? CustomerId { get; set; }
        [Parameter] public bool IsEmbeddedModeActive { get; set; } = false;
        
        private List<MaterialUsageDTO> materials = new();
        private IEnumerable<Customer> Customers { get; set; } = [];
        public string TenantAccountingState { get; set; }
        private string CustomerState { get; set; }
        private bool HasAnyChanges => HasChanges || (materials?.Any(m => m.IsNew || m.IsDeleted) ?? false);

        private static readonly IList<JobStatus> JobStatuses = [.. Enum.GetValues<JobStatus>()];

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new() { VoucherType = VoucherType.Job, VoucherDate = DateTimeOffset.UtcNow };

                // If CustomerId is provided, set it on the new job
                if (CustomerId.HasValue)
                {
                    CurrentObject.PartyId = CustomerId.Value;
                }

                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await JobApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
                await LoadMaterials();
            }
            var result = await TenantAccountingSettingsApiService.Get(top: 1);
            var accSetings = result?.Value?.FirstOrDefault(f => !string.IsNullOrEmpty(f.CompanyState));
            TenantAccountingState = accSetings?.CompanyState;
            await LoadCustoers();
            await InitEditContext();
        }

        protected string GetExpandString()
        {
            return new ODataExpand<Job>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Expand(f => f.Contact, f => f.Contact.FirstName)
                .Build();
        }

        private async Task LoadMaterials()
        {
            try
            {
                if (Oid != Guid.Empty)
                {
                    materials = await JobApiService.GetMaterialsForEditingAsync(Oid);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["FailedToLoadMaterials"]
                });
            }
        }
        private async Task LoadCustoers()
        {
            try
            {
                var result = await CustomerApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                Customers = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadCustomersFailed"] });
            }
        }

        private void OnMaterialsChanged(List<MaterialUsageDTO> updatedMaterials)
        {
            CustomerState = Customers.FirstOrDefault(c => c.Oid == CurrentObject.PartyId)?.State;
            if (updatedMaterials.Any(i => i.InventoryItemId == null)) return;
            materials = updatedMaterials;
            CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            var subTotal = materials.Where(m => !m.IsDeleted).Sum(m => m.Total);
            var totalDiscount = materials.Where(m => !m.IsDeleted).Sum(m => m.DiscountAmount);
            var totalTax = materials.Where(m => !m.IsDeleted).Sum(m => m.TaxAmount ?? 0);

            // Calculate GST breakdown from materials
            decimal cgstAmount = 0;
            decimal sgstAmount = 0;
            decimal igstAmount = 0;
            decimal cessAmount = 0;

            foreach (var material in materials.Where(m => !m.IsDeleted))
            {
                // Calculate taxable amount for this line (after discount)
                var taxableAmount = material.Total - material.DiscountAmount;

                // Calculate GST components based on rates from TaxCode
                if (TenantAccountingState == CustomerState)
                {
                    cgstAmount += taxableAmount * (decimal)(material.CGSTRate / 100);
                    sgstAmount += taxableAmount * (decimal)(material.SGSTRate / 100);
                }
                else
                {
                    igstAmount += taxableAmount * (decimal)(material.IGSTRate / 100);
                }
                cessAmount += taxableAmount * (decimal)(material.CessRate / 100);
            }

            CurrentObject.SubTotal = subTotal;
            CurrentObject.DiscountAmount = totalDiscount;
            CurrentObject.TaxAmount = Math.Round(totalTax);
            CurrentObject.CGSTAmount = cgstAmount;
            CurrentObject.SGSTAmount = sgstAmount;
            CurrentObject.IGSTAmount = igstAmount;
            CurrentObject.CessAmount = cessAmount;
            CurrentObject.TotalAmount = Math.Round(subTotal - totalDiscount + totalTax);
        }

        private async Task OnDiscountTypeChanged(DiscountType type)
        {
                CurrentObject.DiscountType = type;
                StateHasChanged();
        }
        private async Task Changed(JobStatus status)
        {

            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
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
        protected async Task FormSubmit()
        {
            if (materials.Any() && materials.FirstOrDefault().InventoryItemId != null)
            {
                await SaveBulkJob();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Failed"],
                    Detail = Localizer["At least one Job Item is required.."]
                });
            }
        }
        private async Task SaveBulkJob()
        {
            if (CurrentObject == null) return;

            var isValid = EditContext.Validate();
            if (!isValid) return;

            IsBusy = true;
            try
            {
                // Prepare bulk save DTO with job and materials
                var bulkData = new JobBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    Title = CurrentObject.Title,
                    Description = CurrentObject.Description,
                    Status = CurrentObject.Status,
                    Priority = CurrentObject.Priority,
                    PartyId = CurrentObject.PartyId,
                    ContactId = CurrentObject.ContactId,
                    TotalAmount = CurrentObject.TotalAmount,
                    SubTotal = CurrentObject.SubTotal,
                    TaxAmount = CurrentObject.TaxAmount,
                    CGSTAmount = CurrentObject.CGSTAmount,
                    SGSTAmount = CurrentObject.SGSTAmount,
                    IGSTAmount = CurrentObject.IGSTAmount,
                    CessAmount = CurrentObject.CessAmount,
                    DiscountAmount = CurrentObject.DiscountAmount,
                    DiscountPercentage = CurrentObject.DiscountPercent,
                    VoucherDate = CurrentObject.VoucherDate,
                    Materials = materials.Select(m => new MaterialUsageDTO
                    {
                        Oid = m.Oid,
                        InventoryItemId = m.InventoryItemId,
                        Quantity = m.Quantity,
                        UnitPrice = m.UnitPrice,
                        DiscountAmount = m.DiscountAmount,
                        TaxAmount = m.TaxAmount,
                        TaxCodeId = m.TaxCodeId,
                        CGSTRate = m.CGSTRate,
                        SGSTRate = m.SGSTRate,
                        IGSTRate = m.IGSTRate,
                        CessRate = m.CessRate,
                        ChargedAmount = m.ChargedAmount,
                        WaivedAmount = m.WaivedAmount,
                        IsDeleted = m.IsDeleted
                    }).ToList()
                };

                var savedJob = await JobApiService.BulkSaveJobWithMaterialsAsync(bulkData);

                if (savedJob != null)
                {
                    CurrentObject = savedJob;
                    await EnableReadOnlyModeAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = Localizer["Success"],
                        Detail = Localizer["JobSavedSuccessfully"]
                    });
                    DialogService.CloseDialog(CurrentObject);
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
            if (Oid != Guid.Empty)
            {
                await LoadMaterials();
            }
        }
    }
}
