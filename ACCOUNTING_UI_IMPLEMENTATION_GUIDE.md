# Accounting Entities UI Implementation Guide

## Completed Components ✅

### Backend (All Complete)
- ✅ All Server Services (9 files)
- ✅ All Controllers (9 files)
- ✅ All Client API Services (9 files)
- ✅ All Validators (6 files)

### UI (Partially Complete)
- ✅ BankAccount ListView + DetailView

## Remaining UI Components to Create

### 1. Vendor (ListView + DetailView)

**Files to Create:**
- `Client/Pages/ListView/Vendors.razor`
- `Client/Pages/ListView/Vendors.razor.cs`
- `Client/Pages/DetailView/EditVendor.razor`
- `Client/Pages/DetailView/EditVendor.razor.cs`

**ListView Columns:**
- Name, Code, Type, Industry, Email, Phone, ContactPerson, Rating, Status

**DetailView Fields:**
- Name*, Code*, Type, Industry, Email, Phone, Website
- Address, City, State, PostalCode, Country
- TaxId, ContactPerson, Rating, PaymentTermsDays
- Status, Description

**Code Template for Vendors.razor:**
```razor
@page "/vendors"
@using Vanigam.CRM.Objects.Entities
@inherits Vanigam.CRM.Client.Components.BaseListView<Vendor, Vendors>
@attribute [Authorize(Policy = Vanigam.CRM.Objects.ApplicationPolicy.IsAdministrator)]
@inject VendorApiService VendorApiService

<RadzenStack>
    <ListPageTitleComponent TitleText=@Localizer["Vendors"] AddButtonClick=@AddButtonClick SearchButtonClick=@Search />
    <RadzenRow>
        <RadzenColumn SizeMD=12 class="datagrid-container-standard">
            <VanigamAccountingDataGrid @ref="GridControl" AllowColumnPicking="@AllowColPick" Data="@DataSource" Count=Count TItem="Vendor" VanigamAccountingLoadData=@GridLoadData RowDoubleClick="@EditRow" @bind-Settings="@Settings" PageSize="@PageSize" PageSizeOptions="@PageSizeOptions" LoadSettings="@LoadSettings">
                <EmptyTemplate>
                    <NoRecordComponent ShowAddButton="false" />
                </EmptyTemplate>
                <Columns>
                    <RadzenDataGridColumn TItem="Vendor" Filterable="false" Sortable="false" Width="120px" Title="@Localizer["Actions"]">
                        <Template Context="vendor">
                            <OpenPageCommonComponent T="Vendor" OpenObject="@vendor" Open="@Open"></OpenPageCommonComponent>
                            <DeletePageCommonComponent T="Vendor" UsingObject="@vendor" GridDeleteButtonClick="@GridDeleteButtonClick"></DeletePageCommonComponent>
                        </Template>
                        <FooterTemplate>@Localizer["Count"]: <b>@Count</b></FooterTemplate>
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Name) Title=@Localizer["Name"]>
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Code) Title=@Localizer["Code"] Width="100px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Type) Title=@Localizer["Type"] Width="100px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Industry) Title=@Localizer["Industry"] Width="150px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Email) Title=@Localizer["Email"] Width="200px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Phone) Title=@Localizer["Phone"] Width="150px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.ContactPerson) Title=@Localizer["ContactPerson"] Width="150px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Status) Title=@Localizer["Status"] Width="100px">
                    </RadzenDataGridColumn>
                    <RadzenDataGridColumn TItem="Vendor" Property=@nameof(Vendor.Oid) Title=@Localizer["Oid"] Visible="false">
                    </RadzenDataGridColumn>
                </Columns>
            </VanigamAccountingDataGrid>
        </RadzenColumn>
    </RadzenRow>
</RadzenStack>
```

**Code Template for EditVendor.razor.cs:**
```csharp
protected override async Task OnInitializedAsync()
{
    if (Oid == Guid.Empty)
        CurrentObject = new() { AccountType = Objects.Entities.AccountType.Vendor };
    else
        CurrentObject = await VendorApiService.GetByOid(oid: Oid);

    await InitEditContext();
}
```

### 2. PurchaseOrder (ListView + DetailView)

**Files to Create:**
- `Client/Pages/ListView/PurchaseOrders.razor`
- `Client/Pages/ListView/PurchaseOrders.razor.cs`
- `Client/Pages/DetailView/EditPurchaseOrder.razor`
- `Client/Pages/DetailView/EditPurchaseOrder.razor.cs`

**ListView Columns:**
- Number, Status, VendorName (expand), VoucherDate, DueDate, ExpectedDeliveryDate, TotalAmount

**DetailView Fields:**
- Number*, Status, VendorId* (dropdown), VoucherDate, DueDate
- ExpectedDeliveryDate, ShippingAddress, ContactPerson
- Reference, Notes, Terms
- SubTotal, TaxAmount, DiscountAmount, TotalAmount

**Special Notes:**
- Include VoucherLines/PurchaseOrderItems grid in DetailView tabs
- Use expand: "Vendor" in ListView
- VoucherType should be automatically set to PurchaseOrder

### 3. PurchaseInvoice (ListView + DetailView)

**Files to Create:**
- `Client/Pages/ListView/PurchaseInvoices.razor`
- `Client/Pages/ListView/PurchaseInvoices.razor.cs`
- `Client/Pages/DetailView/EditPurchaseInvoice.razor`
- `Client/Pages/DetailView/EditPurchaseInvoice.razor.cs`

**ListView Columns:**
- Number, Status, VendorName (expand), VendorInvoiceNumber, VoucherDate, ReceivedDate, TotalAmount

**DetailView Fields:**
- Number*, Status, VendorId* (dropdown), PurchaseOrderId (dropdown)
- VendorInvoiceNumber, VoucherDate, DueDate, ReceivedDate
- Reference, Notes, Terms
- SubTotal, TaxAmount, DiscountAmount, TotalAmount

**Special Notes:**
- Include PurchaseInvoiceItems grid in DetailView tabs
- Include Payments tab
- VoucherType should be automatically set to PurchaseInvoice

### 4. LedgerEntry (ListView + DetailView)

**Files to Create:**
- `Client/Pages/ListView/LedgerEntries.razor`
- `Client/Pages/ListView/LedgerEntries.razor.cs`
- `Client/Pages/DetailView/EditLedgerEntry.razor`
- `Client/Pages/DetailView/EditLedgerEntry.razor.cs`

**ListView Columns:**
- EntryNumber, EntryDate, EntryType, Amount, DebitAccount (expand), CreditAccount (expand), IsReconciled

**DetailView Fields:**
- EntryNumber*, EntryDate, EntryType*
- Amount*, Description, Reference
- VoucherId (lookup to Voucher)
- DebitAccountId (dropdown), CreditAccountId (dropdown)
- IsReconciled, ReconciledDate, ReconciledBy

**Special Notes:**
- Filter to show only unreconciled entries by default
- Include reconciliation button/functionality

### 5. StockLedgerEntry (ListView + DetailView)

**Files to Create:**
- `Client/Pages/ListView/StockLedgerEntries.razor`
- `Client/Pages/ListView/StockLedgerEntries.razor.cs`
- `Client/Pages/DetailView/EditStockLedgerEntry.razor`
- `Client/Pages/DetailView/EditStockLedgerEntry.razor.cs`

**ListView Columns:**
- EntryNumber, EntryDate, MovementType, InventoryItemName (expand), LocationName (expand), QuantityIn, QuantityOut, Balance

**DetailView Fields:**
- EntryNumber*, EntryDate, MovementType*
- InventoryItemId* (dropdown), LocationId (dropdown)
- QuantityIn, QuantityOut, Balance (calculated)
- UnitCost, TotalValue (calculated)
- Description, Reference
- VoucherId (lookup), VoucherLineId (lookup)
- BatchNumber, ExpiryDate

**Special Notes:**
- Balance should auto-calculate: PreviousBalance + QuantityIn - QuantityOut
- TotalValue = Quantity * UnitCost

## Implementation Pattern Summary

### ListView Pattern (.razor)
```razor
@page "/{entityname}s"
@using Vanigam.CRM.Objects.Entities
@inherits Vanigam.CRM.Client.Components.BaseListView<{EntityName}, {EntityName}s>
@attribute [Authorize(Policy = Vanigam.CRM.Objects.ApplicationPolicy.IsAdministrator)]
@inject {EntityName}ApiService {EntityName}ApiService
```

### ListView Code-Behind Pattern (.razor.cs)
```csharp
namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class {EntityName}s
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await {EntityName}ApiService.Get(
                    filter: GetFilterString(args),
                    expand: GetExpandString(args),
                    orderBy: $"{args.OrderBy}",
                    top: args.Top,
                    skip: args.Skip,
                    count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage() {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer[$"Error"],
                    Detail = Localizer[$"Load"]
                });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<{EntityName}>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.{SearchableProperty1}, SearchString)
                .ContainsOr(u => u.{SearchableProperty2}, SearchString)
                .EndGroup()
                .Build();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
            return "{NavigationProperty1},{NavigationProperty2}"; // or string.Empty
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<Edit{EntityName}>(
                Localizer["Add{EntityName}"], null, 80, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<{EntityName}> args)
        {
            await Open(args.Data);
        }

        private async Task Open({EntityName} entity)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<Edit{EntityName}>(
                Localizer["Edit{EntityName}"],
                new Dictionary<string, object> { { "Oid", entity.Oid } },
                80, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick({EntityName} entity)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await {EntityName}ApiService.Delete(oid: entity.Oid);
                    if (deleteResult != null)
                    {
                        await GridReload();
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = Localizer[$"Success"],
                            Detail = Localizer[$"SuccessfullyDeleted"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer[$"Error"],
                    Detail = Localizer[$"UnableDelete"]
                });
            }
        }
    }
}
```

### DetailView Pattern (.razor)
```razor
@page "/edit-{entityname}"
@using Vanigam.CRM.Objects.Entities
@using Vanigam.CRM.Client.Validators
@inherits Vanigam.CRM.Client.Components.BaseDetailView<{EntityName}, Edit{EntityName}>
@attribute [Authorize(Policy = Vanigam.CRM.Objects.ApplicationPolicy.IsAdministrator)]

<DetailPageTitleComponent TitleText="@Localizer["Edit{EntityName}"]"
                          DialogService="@DialogService"
                          CanEdit="@CanEdit"
                          HasChanges="@HasChanges"
                          CurrentOid="@Oid">
    <CustomBadge>
        @if (IsEditButtonVisible)
        {
            <VanigamEditButton Click="@EnableEditMode" Title="@Localizer["Edit"]"/>
        }
    </CustomBadge>
</DetailPageTitleComponent>

<RadzenColumn SizeMD=12>
    <RadzenAlert ... ErrorVisible />
    <RadzenAlert ... ShowNotUniqueAlert />

    @* Read-Only Mode *@
    @if (IsReadOnlyModeVisible)
    {
        <RadzenCard class="rz-my-4">
            <RadzenStack>
                <div class="read-only-section-header">@Localizer["SectionName"]</div>
                <RadzenRow>
                    <RadzenColumn Size="6">
                        <div class="rz-p-4">
                            <div class="read-only-field rz-mb-3">
                                <RadzenText TextStyle="TextStyle.Subtitle2" TagName="TagName.Span" class="field-label">
                                    <strong>@Localizer["FieldName"]:</strong>
                                </RadzenText>
                                <RadzenText TextStyle="TextStyle.Body1" TagName="TagName.Span" class="field-value">
                                    @(CurrentObject.FieldValue ?? "-")
                                </RadzenText>
                            </div>
                        </div>
                    </RadzenColumn>
                </RadzenRow>
            </RadzenStack>
        </RadzenCard>
    }

    @* Editable Form Mode *@
    <RadzenTemplateForm @ref=Form EditContext="EditContext" TItem="{EntityName}"
                        Data="@CurrentObject" Visible="@IsFormVisible" Submit="@SaveAndStayInEdit">
        <RadzenStack>
            <FluentValidationValidator Validator="new {EntityName}Validator(Localizer)" />
            <ValidationSummary />

            <VanigamAccountingFormField Text=@Localizer["FieldName"]>
                <ChildContent>
                    <VanigamAccountingTextBox @bind-Value="@CurrentObject.FieldName" Name="txt_FieldName" />
                </ChildContent>
            </VanigamAccountingFormField>

            @* Add more form fields *@
        </RadzenStack>
        <RadzenStack ... SaveButtons />
    </RadzenTemplateForm>
</RadzenColumn>
```

### DetailView Code-Behind Pattern (.razor.cs)
```csharp
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class Edit{EntityName}
    {
        [Inject] private {EntityName}ApiService {EntityName}ApiService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
                CurrentObject = new();
            else
                CurrentObject = await {EntityName}ApiService.GetByOid(oid: Oid);

            await InitEditContext();
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await {EntityName}ApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await {EntityName}ApiService.Update(oid: Oid, CurrentObject);
                    if (result.IsPreconditionFailed())
                    {
                        HasChanges = true;
                        CanEdit = false;
                        return;
                    }
                }
                NotificationService.Notify(new NotificationMessage {
                    Severity = NotificationSeverity.Success,
                    Summary = Localizer["SavedSuccessfully!"]
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
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
            }
            IsBusy = false;
        }
    }
}
```

## Common Form Field Types

### Text Input
```razor
<VanigamAccountingFormField Text=@Localizer["FieldName"]>
    <ChildContent>
        <VanigamAccountingTextBox @bind-Value="@CurrentObject.FieldName" Name="txt_FieldName" />
    </ChildContent>
</VanigamAccountingFormField>
```

### Numeric Input
```razor
<VanigamAccountingFormField Text=@Localizer["Amount"]>
    <ChildContent>
        <RadzenNumeric @bind-Value="@CurrentObject.Amount" Name="txt_Amount" />
    </ChildContent>
</VanigamAccountingFormField>
```

### Date Input
```razor
<VanigamAccountingFormField Text=@Localizer["Date"]>
    <ChildContent>
        <RadzenDatePicker @bind-Value="@CurrentObject.Date" Name="txt_Date" />
    </ChildContent>
</VanigamAccountingFormField>
```

### Dropdown/Enum
```razor
<VanigamAccountingFormField Text=@Localizer["Status"]>
    <ChildContent>
        <{EnumName}EnumDropDown @bind-Value="@CurrentObject.Status" />
    </ChildContent>
</VanigamAccountingFormField>
```

### Checkbox
```razor
<VanigamAccountingFormField Text=@Localizer["IsActive"]>
    <ChildContent>
        <RadzenCheckBox @bind-Value="@CurrentObject.IsActive" Name="chk_IsActive" />
    </ChildContent>
</VanigamAccountingFormField>
```

### TextArea
```razor
<VanigamAccountingFormField Text=@Localizer["Description"]>
    <ChildContent>
        <RadzenTextArea @bind-Value="@CurrentObject.Description" Name="txt_Description" Rows="3" />
    </ChildContent>
</VanigamAccountingFormField>
```

## Quick Reference

**Files Created So Far:** 33 backend files + 2 UI components (BankAccount)
**Files Remaining:** 10 UI files (5 entities × 2 files each)

**Next Steps:**
1. Create Vendor ListView + DetailView
2. Create PurchaseOrder ListView + DetailView
3. Create PurchaseInvoice ListView + DetailView
4. Create LedgerEntry ListView + DetailView
5. Create StockLedgerEntry ListView + DetailView

**Testing Checklist:**
- [ ] All ListViews load correctly
- [ ] Grid filtering and sorting works
- [ ] Add/Edit dialogs open correctly
- [ ] Validation works for required fields
- [ ] Save/Update operations succeed
- [ ] Delete operations work with confirmation
- [ ] Read-only mode displays all fields correctly
- [ ] Edit mode allows modification of fields
