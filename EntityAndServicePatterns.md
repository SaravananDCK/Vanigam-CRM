# Entity and Service Patterns

This document outlines the standardized patterns for implementing entities, services, controllers, and UI components in the Vanigam CRM application.

## Base Entity Hierarchy

```
BaseClass (Objects/Contracts/BaseClass.cs)
├── IHasId (Guid Oid primary key)
├── IHasAudit (Created/Updated tracking)
├── IHasSoftDelete (IsNotDeleted flag)
└── ITenant (TenantId for multi-tenancy)

NamedClass : BaseClass + IName (common base for named entities)
CodedClass : BaseClass + IName (entities with Code + Name pattern)
```

## Server Services Pattern

- **Base**: `BaseService<T>` in `Server/Services/BaseService.cs`
- **Auto-tenant filtering**: `ApplyUserRoleFilter()` method
- **Dependency injection**: All services auto-registered via `AddInheritedClasses()`

**Service Template** (follow this exact pattern):
```csharp
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class CustomerService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Customer>> logger)
    : BaseService<Customer>(context, logger)
{
    public override DbSet<Customer> GetDbSet()
    {
        return Context.Customers;
    }
}
```

**Service Naming Convention**: `{EntityName}Service` (e.g., `CustomerService`, `JobService`)
**File Location**: `Server/Services/{EntityName}Service.cs`
**Required**: Every entity must have a corresponding service for proper dependency injection

## Client API Services Pattern

- **Base**: `BaseApiService<T>` in `Client/Services/BaseApiService.cs`
- **OData integration**: Built-in support for OData queries
- **Authentication**: Automatic Bearer token handling

**Client API Service Template** (follow this exact pattern):
```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class CustomerApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Customer>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Customers));
```

**API Service Naming Convention**: `{EntityName}ApiService` (e.g., `CustomerApiService`, `JobApiService`)
**File Location**: `Client/Services/{EntityName}ApiService.cs`
**DbContext Reference**: Use `nameof(VanigamAccountingDbContext.{EntityName}s)` for the controller name
**Required**: Every entity should have a corresponding API service for client-side data access

## OData Controller Pattern

- **Base**: `BaseODataServiceController<T, K>` for OData endpoints
- **Authentication**: JWT Bearer token required
- **Auto-service injection**: Service type K automatically injected
- **Standard CRUD**: GET, POST, PUT, PATCH, DELETE operations

**OData Controller Template** (follow this exact pattern):
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Customers)}")]
    public class CustomersController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    CustomerService service)
    : BaseODataServiceController<Customer, CustomerService>(context, userManager, roleManager,
        service, null);
}
```

**Controller Naming Convention**: `{EntityName}sController` (plural, e.g., `CustomersController`, `JobsController`)
**File Location**: `Server/Controllers/MeditalkAIService/{EntityName}sController.cs`
**Route Pattern**: `odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.{EntityName}s)}`
**Required**: Every entity must have a corresponding OData controller for API access

## Blazor ListView Pattern

- **Base**: Components inherit from `BaseListView<T, TPage>`
- **Location**: `Client/Pages/ListView/{EntityName}s.razor` (plural)
- **Code-behind**: `Client/Pages/ListView/{EntityName}s.razor.cs`
- **Authentication**: Use appropriate `[Authorize]` attributes
- **Grid**: Uses `VanigamAccountingDataGrid` with standard columns

**ListView Template** (follow this exact pattern):
```razor
@page "/{entityname}s"
@using Vanigam.CRM.Objects.Entities
@inherits Vanigam.CRM.Client.Components.BaseListView<{EntityName}, {EntityName}s>
@attribute [Authorize(Policy = Vanigam.CRM.Objects.ApplicationPolicy.IsAdministrator)]
@inject {EntityName}ApiService {EntityName}ApiService

<RadzenStack>
    <ListPageTitleComponent TitleText=@Localizer["{EntityName}s"] AddButtonClick=@AddButtonClick SearchButtonClick=@Search />
    <RadzenRow>
        <RadzenColumn SizeMD=12 class="datagrid-container-standard">
            <VanigamAccountingDataGrid @ref="GridControl" AllowColumnPicking="@AllowColPick" Data="@DataSource" Count=Count TItem="{EntityName}" VanigamAccountingLoadData=@GridLoadData RowDoubleClick="@EditRow" @bind-Settings="@Settings" PageSize="@PageSize" PageSizeOptions="@PageSizeOptions" LoadSettings="@LoadSettings">
                <EmptyTemplate>
                    <NoRecordComponent ShowAddButton="false" />
                </EmptyTemplate>
                <Columns>
                    <RadzenDataGridColumn TItem="{EntityName}" Filterable="false" Sortable="false" Width="120px" Title="@Localizer["Actions"]">
                        <Template Context="{entityname}">
                            <OpenPageCommonComponent T="{EntityName}" OpenObject="@{entityname}" Open="@Open"></OpenPageCommonComponent>
                            <DeletePageCommonComponent T="{EntityName}" UsingObject="@{entityname}" GridDeleteButtonClick="@GridDeleteButtonClick"></DeletePageCommonComponent>
                        </Template>
                        <FooterTemplate>@Localizer["Count"]: <b>@Count</b></FooterTemplate>
                    </RadzenDataGridColumn>
                    <!-- Add entity-specific columns here -->
                    <RadzenDataGridColumn TItem="{EntityName}" Property=@nameof({EntityName}.Oid) Title=@Localizer["Oid"] Visible="false">
                    </RadzenDataGridColumn>
                </Columns>
            </VanigamAccountingDataGrid>
        </RadzenColumn>
    </RadzenRow>
</RadzenStack>
```

**ListView Code-behind Template** (follow this exact pattern):
```csharp
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class {EntityName}s
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await {EntityName}ApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<{EntityName}>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                // Add searchable properties here
                .ContainsOr(u => u.Name, SearchString) // Example
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<Edit{EntityName}>(Localizer["Add{EntityName}"], null, 30, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<{EntityName}> args)
        {
            await Open(args.Data);
        }

        private async Task Open({EntityName} {entityname})
        {
            await DialogService.OpenDialogAsync<Edit{EntityName}>(Localizer["Edit{EntityName}"], new Dictionary<string, object> { { "Oid", {entityname}.Oid } }, 30, 50);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick({EntityName} {entityname})
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await {EntityName}ApiService.Delete(oid:{entityname}.Oid);

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

## Blazor DetailView Pattern

- **Base**: Components inherit from `BaseDetailView<T, TPage>`
- **Location**: `Client/Pages/DetailView/Edit{EntityName}.razor`
- **Code-behind**: `Client/Pages/DetailView/Edit{EntityName}.razor.cs`
- **Validator**: `Client/Validators/{EntityName}Validator.cs` (required)
- **Authentication**: Use appropriate `[Authorize]` attributes
- **Validation**: Uses FluentValidation with entity-specific validators

**DetailView Template** (follow this exact pattern):
```razor
@page "/edit-{entityname}"
@using Vanigam.CRM.Objects.Entities
@using Vanigam.CRM.Client.Validators
@inherits Vanigam.CRM.Client.Components.BaseDetailView<{EntityName}, Edit{EntityName}>
@attribute [Authorize(Policy = Vanigam.CRM.Objects.ApplicationPolicy.IsAdministrator)]

@* Custom Header with Edit Button *@
<DetailPageTitleComponent TitleText="@Localizer["Edit{EntityName}"]" DialogService="@DialogService" CanEdit="@CanEdit" HasChanges="@HasChanges" CurrentOid="@Oid">
    <CustomBadge>
        @* Floating Edit Button - Only visible in read-only mode *@
        @if (IsEditButtonVisible)
        {
            <VanigamEditButton Click="@EnableEditMode" Title="@Localizer["Edit"]"/>
        }
    </CustomBadge>
</DetailPageTitleComponent>

<RadzenColumn SizeMD=12>
    <RadzenAlert Shade="Shade.Lighter" Variant="Variant.Flat" Size="AlertSize.Small" AlertStyle="AlertStyle.Danger" Visible="@ErrorVisible">@Localizer["SaveAlert"]</RadzenAlert>
    <RadzenAlert Shade="Shade.Lighter" Variant="Variant.Flat" Size="AlertSize.Small" AlertStyle="AlertStyle.Warning" Visible="@ShowNotUniqueAlert">@Localizer["CodeMust"]</RadzenAlert>

    @* Read-Only Mode Display *@
    @if (IsReadOnlyModeVisible)
    {
        <RadzenCard class="rz-my-4">
            <RadzenStack>
                @* Section Header Example *@
                <div class="read-only-section-header">@Localizer["{EntityName}Information"]</div>
                <RadzenRow>
                    <RadzenColumn Size="6">
                        <div class="rz-p-4">
                            <div class="read-only-field rz-mb-3">
                                <RadzenText TextStyle="TextStyle.Subtitle2" TagName="TagName.Span" class="field-label"><strong>@Localizer["FieldName"]:</strong></RadzenText>
                                <RadzenText TextStyle="TextStyle.Body1" TagName="TagName.Span" class="field-value">@(CurrentObject.FieldValue ?? "-")</RadzenText>
                            </div>
                            <!-- Add more read-only fields here -->
                        </div>
                    </RadzenColumn>
                    <RadzenColumn Size="6">
                        <div class="rz-p-4">
                            <!-- Additional fields in second column -->
                        </div>
                    </RadzenColumn>
                </RadzenRow>
            </RadzenStack>
        </RadzenCard>
    }

    @* Editable Form Mode *@
    <RadzenTemplateForm @ref=Form EditContext="EditContext" TItem="{EntityName}" Data="@CurrentObject" Visible="@IsFormVisible" Submit="@SaveAndStayInEdit">
        <RadzenStack>
            <FluentValidationValidator Validator="new {EntityName}Validator(Localizer)" />
            <ValidationSummary />
            <!-- Add entity-specific form fields here -->
        </RadzenStack>
        <RadzenStack Style="margin-top:1rem;" Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.End" Gap="0.5rem">
            @if (IsCreateMode)
            {
                <VanigamAccountingSaveButton Id="btn_Save" Text="@Localizer["Save"]" Disabled="@IsFormUnmodified" @bind-IsBusy="IsBusy" />
            }
            else
            {
                <VanigamAccountingSaveButton Id="btn_Update" Text="@Localizer["Update"]" Disabled="@IsFormUnmodified" @bind-IsBusy="IsBusy" />
                <VanigamAccountingCancelButton Text="@Localizer["Cancel"]" Click="@EnableReadOnlyMode" />
            }
        </RadzenStack>
    </RadzenTemplateForm>

    @* Related Data Tabs - Available in both read-only and edit modes *@
    @if (IsTabsVisible)
    {
        <RadzenCard class="mt-4">
            <RadzenTabs @bind-SelectedIndex="SelectedTabIndex" RenderMode="TabRenderMode.Client">
                <Tabs>
                    <!-- Add tabs for related entities here -->
                </Tabs>
            </RadzenTabs>
        </RadzenCard>
    }
</RadzenColumn>
```

**DetailView Code-behind Template** (follow this exact pattern):
```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;

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
    }
}
```

**FluentValidation Validator Template** (required for DetailView):
```csharp
using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class {EntityName}Validator : AbstractValidator<{EntityName}>
    {
        public {EntityName}Validator(IStringLocalizer localizer)
        {
            // Add validation rules for required fields
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]); // Example
            // Add additional validation rules as needed
        }
    }
}
```

**Page Naming Conventions**:
- **ListView**: `{EntityName}s.razor` (plural, e.g., `Customers.razor`, `Jobs.razor`)
- **DetailView**: `Edit{EntityName}.razor` (e.g., `EditCustomer.razor`, `EditJob.razor`)
- **Route**: ListView uses `/{entityname}s`, DetailView uses `/edit-{entityname}` (lowercase)

## DetailView Read-Only Mode Formatting Standards

**IMPORTANT**: All DetailView pages must implement consistent read-only mode formatting using the following patterns:

**CSS Classes Used**:
- `.read-only-section-header` - For section headers (replaces `RadzenText TextStyle="H6"`)
- `.read-only-field` - Container for label/value pairs
- `.field-label` - For field labels (automatically bold)
- `.field-value` - For field values

**Section Header Pattern**:
```razor
@* Replace RadzenText H6 with this *@
<div class="read-only-section-header">@Localizer["SectionName"]</div>
```

**Field Display Pattern**:
```razor
@* Replace separate RadzenText elements with this *@
<div class="read-only-field rz-mb-3">
    <RadzenText TextStyle="TextStyle.Subtitle2" TagName="TagName.Span" class="field-label">
        <strong>@Localizer["FieldName"]:</strong>
    </RadzenText>
    <RadzenText TextStyle="TextStyle.Body1" TagName="TagName.Span" class="field-value">
        @(CurrentObject.FieldValue ?? "-")
    </RadzenText>
</div>
```

**Key Requirements**:
- **Labels and values in same row**: Use flexbox layout with `.read-only-field`
- **Bold labels with colon**: Always use `<strong>@Localizer["FieldName"]:</strong>`
- **Consistent spacing**: Use `rz-mb-3` class for field spacing
- **Section headers**: Use `.read-only-section-header` class instead of RadzenText H6
- **Mobile responsive**: Automatically stacks vertically on mobile devices
- **Null handling**: Always provide fallback with `?? "-"` for nullable values

**Before (incorrect)**:
```razor
<RadzenText TextStyle="TextStyle.H6" class="rz-mb-4">@Localizer["Section"]</RadzenText>
<RadzenText TextStyle="TextStyle.Subtitle1" class="rz-mb-2">@Localizer["Name"]</RadzenText>
<RadzenText TextStyle="TextStyle.Body1" class="rz-mb-4">@(CurrentObject.Name ?? "-")</RadzenText>
```

**After (correct)**:
```razor
<div class="read-only-section-header">@Localizer["Section"]</div>
<div class="read-only-field rz-mb-3">
    <RadzenText TextStyle="TextStyle.Subtitle2" TagName="TagName.Span" class="field-label">
        <strong>@Localizer["Name"]:</strong>
    </RadzenText>
    <RadzenText TextStyle="TextStyle.Body1" TagName="TagName.Span" class="field-value">
        @(CurrentObject.Name ?? "-")
    </RadzenText>
</div>
```