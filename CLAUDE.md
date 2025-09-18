# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
- **Build entire solution**: `dotnet build`
- **Run server in development**: `dotnet run --project Server`
  - Server runs on https://localhost:61564 and http://localhost:61565
- **Run specific project**: `dotnet run --project Client` or `dotnet run --project Server`
- **Clean solution**: `dotnet clean`
- **Restore packages**: `dotnet restore`

### Database Operations
- **Add migration**: `dotnet ef migrations add <MigrationName> --project Objects --startup-project Server`
- **Update database**: `dotnet ef database update --project Objects --startup-project Server`
- **Generate SQL script**: `dotnet ef migrations script --project Objects --startup-project Server`

### Entity Framework Code First
- The main DbContext is `VanigamAccountingDbContext` in the Objects project
- Connection strings support both PostgreSQL and SQL Server via `IsPostGreSQL` configuration flag
- Database initialization and seeding happens in `VanigamAccountingDbContext.SeedInitialData()`

## Architecture Overview

### Project Structure
This is a **Blazor Server + WebAssembly hybrid application** with 4 main projects:

1. **Server** (`Vanigam.CRM.Server`) - ASP.NET Core host with Blazor Server and Web API
2. **Client** (`Vanigam.CRM.Client`) - Blazor WebAssembly client
3. **Objects** (`Vanigam.CRM.Objects`) - Shared data models, entities, and DbContext
4. **Reports** (`Vanigam.CRM.Reports`) - DevExpress reporting components

### Key Technologies
- **.NET 8.0** with C# 12
- **Blazor Server + WebAssembly** hybrid mode
- **Entity Framework Core 8** with PostgreSQL/SQL Server support
- **ASP.NET Core Identity** for authentication
- **OData v8** for API endpoints (`/odata/VanigamAccountingService/`)
- **DevExpress Components** for reporting and rich UI controls
- **Radzen Blazor** components
- **Hangfire** for background job processing
- **Serilog** for logging with PostgreSQL sink
- **QuestPDF** for PDF generation
- **SignalR** for real-time communication

### Multi-Tenant Architecture
The application implements **row-level multi-tenancy**:

- All entities inherit from `BaseClass` which implements `ITenant` interface
- `TenantId` property automatically filters data per tenant
- Services inherit from `BaseService<T>` which automatically applies tenant filtering
- User context provided by `ICurrentUserService`
- Entities can be marked with `[NonTenantObject]` attribute to bypass tenant filtering

### Entity and Service Patterns

#### Base Entity Hierarchy
```
BaseClass (Objects/Contracts/BaseClass.cs)
├── IHasId (Guid Oid primary key)
├── IHasAudit (Created/Updated tracking)
├── IHasSoftDelete (IsNotDeleted flag)
└── ITenant (TenantId for multi-tenancy)

NamedClass : BaseClass + IName (common base for named entities)
CodedClass : BaseClass + IName (entities with Code + Name pattern)
```

#### Server Services Pattern
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

#### Client API Services Pattern
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

#### OData Controller Pattern
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

#### Blazor ListView Pattern
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

#### Blazor DetailView Pattern
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

<DetailPageCommonComponent TitleText="@Localizer["Edit{EntityName}"]" DialogService="@DialogService" CanEdit="@CanEdit" HasChanges="@HasChanges" CurrentOid="@Oid" />
<RadzenColumn SizeMD=12>
    <RadzenAlert Shade="Shade.Lighter" Variant="Variant.Flat" Size="AlertSize.Small" AlertStyle="AlertStyle.Danger" Visible="@ErrorVisible">@Localizer["SaveAlert"]</RadzenAlert>
    <RadzenAlert Shade="Shade.Lighter" Variant="Variant.Flat" Size="AlertSize.Small" AlertStyle="AlertStyle.Warning" Visible="@ShowNotUniqueAlert">@Localizer["CodeMust"]</RadzenAlert>
    <RadzenTemplateForm @ref=Form EditContext="EditContext" TItem="{EntityName}" Data="@CurrentObject" Visible="@(CurrentObject != null && CanEdit)" Submit="@FormSubmit">
        <RadzenStack>
            <FluentValidationValidator Validator="new {EntityName}Validator(Localizer)" />
            <ValidationSummary />
            <!-- Add entity-specific form fields here -->
        </RadzenStack>
        <RadzenStack Style="margin-top:1rem;" Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.End" Gap="0.5rem">
            <VanigamAccountingSaveButton Id="btn_Save" Text="@Localizer["Save"]" Disabled="@(!(Form.EditContext.IsModified()))" @bind-IsBusy="IsBusy" />
            <VanigamAccountingCancelButton Id="btn_Cancel" Text="@Localizer["Cancel"]" Click="@CancelButtonClick" />
        </RadzenStack>
    </RadzenTemplateForm>
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

### OData Configuration
OData endpoints configured in `Server/Extensions/ODataExtensions.cs`:
- **Route**: `/odata/VanigamAccountingService/`
- **Features**: Count, Filter, OrderBy, Expand, Select enabled
- **Entity registration**: All entities must be registered as EntitySets
- **Custom functions**: Example tenant user functions implemented

### Database Entities Structure
Located in `Objects/Entities/`, all properly decorated with EF Core data annotations:

**CRM Core**:
- `Customer`, `Contact`, `Lead`, `Opportunity`
- `Job`, `JobAssignment`, `Appointment`, `JobReport`
- `Quote`, `QuoteItem`, `Invoice`, `Payment`

**Operational**:
- `Technician`, `Employee`, `TimeSheet`, `MaterialUsage`
- `InventoryItem`, `Location`, `Vehicle`, `GPSPoint`

**Business Logic**:
- `Contract`, `RecurringJob`, `Sla`, `Feedback`
- `Activity`, `Notification`, `AuditLog`, `CustomField`

**System**:
- `ApplicationUser`, `ApplicationRole`, `ApplicationTenant`
- `UserSession`, `Language`, `DocumentTemplate`, `FileDocument`

### Authentication & Authorization
- **ASP.NET Core Identity** with custom user/role classes
- **JWT Bearer authentication** for API endpoints
- **Cookie authentication** for Blazor Server components
- **Multi-tenant user management**: Users belong to specific tenants
- **Role-based access**: SuperUser, Admin roles implemented

### Background Jobs
- **Hangfire** integration with PostgreSQL storage
- **Job definitions**: Located in `Server/HangFire/`
- **Automatic startup**: Configured in `Program.cs`

### Reporting System
- **DevExpress XtraReports** for advanced reporting
- **Report templates**: PDF, DOCX, and macro-enabled templates
- **Custom storage**: `CustomReportStorageWebExtension` for report management
- **PDF generation**: QuestPDF for simple PDF creation
- **Template engine**: Document template system with field mapping

### Configuration Requirements
- **Connection strings**: Support both PostgreSQL and SQL Server
- **Redis**: Optional caching layer
- **Azure Service Bus**: For external integrations
- **SendGrid/Mailjet**: Email services
- **Twilio**: SMS/Voice services
- **Firebase**: Push notifications
- **Application Insights**: Telemetry and monitoring

### Key Conventions
- **Entity naming**: Pascal case, singular (e.g., `Customer`, not `Customers`)
- **DbSet naming**: Plural (e.g., `DbSet<Customer> Customers`)
- **Foreign keys**: `EntityId` convention with `[ForeignKey]` attributes
- **String lengths**: Appropriate `[StringLength]` attributes on all string properties
- **Required fields**: `[Required]` attributes where appropriate
- **Decimal precision**: `[Column(TypeName = "decimal(18,2)")]` for monetary values
- **Enums**: Stored as strings in database via `.HasConversion<string>()`

### Development Notes
- **Hot reload**: Supported for both Server and Client projects
- **Mixed rendering**: Components can run on Server or Client
- **State management**: Blazor render state handling implemented
- **Localization**: Multi-language support with resource files
- **PWA support**: Service worker configured for offline capabilities
- **DevExpress licensing**: Uses local DLL references in `/References/`

When adding new entities:
1. Create entity class inheriting from `BaseClass` or `NamedClass`/`CodedClass`
2. Add proper EF Core data annotations (`[StringLength]`, `[ForeignKey]`, etc.)
3. Add `DbSet<T>` property to `VanigamAccountingDbContext`
4. Register entity in `ODataExtensions.InitOData()`
5. Create corresponding service inheriting from `BaseService<T>`
6. Create API service inheriting from `BaseApiService<T>` if needed
7. Add migration: `dotnet ef migrations add AddNewEntity --project Objects --startup-project Server`