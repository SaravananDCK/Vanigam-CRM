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
- **Database schema changes**: Use direct SQL scripts instead of EF migrations
- **Schema updates**: Create SQL scripts manually and apply them to the database
- **Database initialization**: Handled through `VanigamAccountingDbContext.SeedInitialData()`
- **Note**: This project does not use EF Core migrations - all schema changes are managed through SQL scripts

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
- **NodaTime** for UTC timestamp generation (uses DateTimeOffset for storage)

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

#### DetailView Read-Only Mode Formatting Standards

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

### DateTime and Timestamp Handling

**IMPORTANT**: This project uses **DateTimeOffset** for all entity properties, NOT NodaTime Instant.

#### Timestamp Standards:
- **Entity Properties**: Use `DateTimeOffset` for all date/time properties
- **Audit Fields**: `CreatedAtUtc` and `UpdatedAtUtc` are `DateTimeOffset?`
- **Activity Dates**: `ActivityDate` is `DateTimeOffset`
- **UTC Generation**: Use `SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()` for creating UTC timestamps
- **PostgreSQL Compatibility**: All DateTimeOffset values must be UTC (offset 0) for PostgreSQL

#### NodaTime Usage Pattern:
```csharp
// CORRECT: Generate UTC DateTimeOffset using NodaTime
entity.CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
entity.ActivityDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

// CORRECT: Convert DateTime to UTC DateTimeOffset using NodaTime
entity.ExpectedCloseDate = Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Utc)).ToDateTimeOffset();

// ALTERNATIVE: Manual UTC conversion (less preferred)
entity.ExpectedCloseDate = new DateTimeOffset(dateTimeValue, TimeSpan.Zero);

// INCORRECT: Don't use Instant directly in entities
entity.CreatedAtUtc = SystemClock.Instance.GetCurrentInstant(); // Wrong!

// INCORRECT: Don't assign DateTime directly to DateTimeOffset (causes PostgreSQL offset errors)
entity.ExpectedCloseDate = dateTimeValue; // Wrong! May have local timezone offset
```

#### Key Rules:
1. **All entities use DateTimeOffset properties**
2. **Generate timestamps via NodaTime but convert to DateTimeOffset**
3. **Always ensure UTC (offset 0) for PostgreSQL compatibility**
4. **Never use DateTime.UtcNow or DateTimeOffset.UtcNow directly**
5. **Use SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset() for all timestamp generation**
6. **When converting DateTime to DateTimeOffset, prefer NodaTime: `Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)).ToDateTimeOffset()`**
7. **Never assign DateTime directly to DateTimeOffset properties (causes PostgreSQL offset errors)**

### Entity Conversion Patterns

The application supports automated conversion between related CRM entities, following business workflow patterns:

#### Lead → Opportunity → Customer Conversion Flow

**Business Logic**:
- **Lead to Opportunity**: Qualified or Contacted leads can be converted to opportunities
- **Opportunity to Customer**: Active opportunities (Proposal, Negotiation, Qualified stages) can be converted to customers
- **Automatic relationships**: Conversions create associated Contact records and Activity tracking

#### Conversion Service Pattern

**Server Conversion Service Template**:
```csharp
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ConversionService(
    VanigamAccountingDbContext context,
    ILogger<ConversionService> logger,
    ICurrentUserService currentUserService)
{
    public async Task<Opportunity> ConvertLeadToOpportunityAsync(
        Guid leadId,
        string opportunityTitle,
        decimal estimatedValue,
        DateTime expectedCloseDate)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Validate business rules
            var lead = await context.Leads.FindAsync(leadId);
            if (lead.Status == LeadStatus.Converted)
                throw new InvalidOperationException("Lead already converted");

            // Create opportunity
            var opportunity = new Opportunity
            {
                Title = opportunityTitle,
                EstimatedValue = estimatedValue,
                ExpectedCloseDate = expectedCloseDate,
                Stage = OpportunityStage.Qualified,
                // Copy relevant fields from lead
            };

            // Update lead status
            lead.Status = LeadStatus.Converted;

            // Create activity record
            var activity = new Activity
            {
                Type = ActivityType.Conversion,
                Description = $"Lead converted to opportunity: {opportunityTitle}",
                // Activity tracking fields
            };

            context.Opportunities.Add(opportunity);
            context.Activities.Add(activity);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return opportunity;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

**Controller Template**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ConversionController(ConversionService conversionService) : ControllerBase
{
    [HttpPost("lead-to-opportunity")]
    public async Task<ActionResult<Opportunity>> ConvertLeadToOpportunity(
        [FromBody] ConvertLeadRequest request)
    {
        try
        {
            var opportunity = await conversionService.ConvertLeadToOpportunityAsync(
                request.LeadId,
                request.Title,
                request.EstimatedValue,
                request.ExpectedCloseDate);
            return Ok(opportunity);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

**Client API Service Template**:
```csharp
public class ConversionApiService(
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<Opportunity?> ConvertLeadToOpportunityAsync(
        Guid leadId,
        string title,
        decimal estimatedValue,
        DateTime expectedCloseDate)
    {
        var request = new ConvertLeadRequest
        {
            LeadId = leadId,
            Title = title,
            EstimatedValue = estimatedValue,
            ExpectedCloseDate = expectedCloseDate
        };

        var response = await httpClient.PostAsJsonAsync("api/conversion/lead-to-opportunity", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Opportunity>();
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(errorMessage);
    }
}
```

#### Conversion Dialog Components Pattern

**Dialog Component Template**:
```razor
@using Vanigam.CRM.Objects.Entities
@using Vanigam.CRM.Client.Services
@inject ConversionApiService ConversionService
@inject NotificationService NotificationService
@inject IStringLocalizer<ConversionDialog> Localizer

<RadzenStack>
    <RadzenText TextStyle="TextStyle.H6">@Localizer["ConversionTitle"]</RadzenText>

    <RadzenTemplateForm @ref="form" TItem="ConversionModel" Data="@model" Submit="@OnSubmit">
        <RadzenStack Gap="1rem">
            <!-- Form fields for conversion parameters -->
            <div>
                <RadzenLabel Text="@Localizer["Title"]" Component="txt_Title" />
                <RadzenTextBox @bind-Value="@model.Title" Name="txt_Title" class="w-100" />
                <RadzenRequiredValidator Component="txt_Title" Text="@Localizer["Required"]" />
            </div>
        </RadzenStack>

        <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.End" class="rz-mt-4">
            <RadzenButton Text="@Localizer["Convert"]" ButtonType="ButtonType.Submit"
                         IsBusy="@isBusy" ButtonStyle="ButtonStyle.Primary" />
            <RadzenButton Text="@Localizer["Cancel"]" Click="@Cancel" ButtonStyle="ButtonStyle.Light" />
        </RadzenStack>
    </RadzenTemplateForm>
</RadzenStack>

@code {
    [Parameter] public TSourceEntity? SourceEntity { get; set; }
    [Parameter] public EventCallback<TTargetEntity> OnConverted { get; set; }
    [Parameter] public EventCallback OnCanceled { get; set; }

    private ConversionModel model = new();
    private bool isBusy = false;

    private async Task OnSubmit()
    {
        isBusy = true;
        try
        {
            var result = await ConversionService.ConvertAsync(SourceEntity.Oid, model);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = Localizer["Success"],
                Detail = Localizer["ConversionSuccessful"]
            });

            await OnConverted.InvokeAsync(result);
        }
        catch (InvalidOperationException ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = Localizer["Warning"],
                Detail = ex.Message
            });
        }
        finally
        {
            isBusy = false;
        }
    }
}
```

#### DetailView Conversion Integration

**Adding Conversion Buttons to DetailView**:
```razor
<DetailPageTitleComponent TitleText="@Localizer["EditLead"]" DialogService="@DialogService" CanEdit="@CanEdit" HasChanges="@HasChanges" CurrentOid="@Oid">
    <CustomBadge>
        @if (IsEditButtonVisible)
        {
            <VanigamEditButton Click="@EnableEditMode" Title="@Localizer["Edit"]"/>
        }
        @* Conversion Button - Only in read-only mode *@
        @if (CurrentObject != null && IsReadOnlyMode && !IsCreateMode && CanConvert)
        {
            <RadzenButton Text="@Localizer["Convert"]"
                         Icon="transform"
                         ButtonStyle="ButtonStyle.Success"
                         Size="ButtonSize.Small"
                         Click="@ShowConversionDialog" />
        }
    </CustomBadge>
</DetailPageTitleComponent>
```

**Business Rule Validation**:
```csharp
// In DetailView code-behind
private bool CanConvertToOpportunity => CurrentObject != null &&
    (CurrentObject.Status == LeadStatus.Qualified || CurrentObject.Status == LeadStatus.Contacted) &&
    CurrentObject.Status != LeadStatus.Converted;

private async Task ShowConversionDialog()
{
    var result = await DialogService.OpenDialogAsync<ConvertLeadToOpportunityDialog>(
        Localizer["ConvertToOpportunity"],
        new Dictionary<string, object> { { "Lead", CurrentObject } },
        50, 40);

    if (result != null)
    {
        // Refresh entity to show updated status
        CurrentObject = await LeadApiService.GetByOid(oid: Oid);
        StateHasChanged();
    }
}
```

#### Service Registration Pattern

**Server Registration** (`Program.cs`):
```csharp
builder.Services.AddScoped<LeadConversionService>();
```

**Client Registration** (`Program.cs`):
```csharp
builder.Services.AddScoped<LeadConversionApiService>();
```

#### Key Conversion Principles

1. **Transaction Safety**: Always use database transactions for conversions
2. **Business Rule Validation**: Validate entity state before conversion
3. **Activity Tracking**: Log conversion activities for audit trail
4. **Status Updates**: Update source entity status to prevent duplicate conversions
5. **Relationship Preservation**: Maintain data relationships (e.g., Contact associations)
6. **Error Handling**: Provide meaningful error messages for business rule violations
7. **UI Feedback**: Show conversion progress and success/failure notifications

#### Conversion Button Placement Standards

- **Location**: CustomBadge section of DetailPageTitleComponent
- **Visibility**: Only in read-only mode for existing entities
- **Icon**: Use "transform" icon for conversion actions
- **Style**: ButtonStyle.Success with Small size
- **Business Rules**: Implement entity-specific conversion eligibility logic

#### Status Filter Bar Pattern

**Overview**: Status filter bar provides a visual interface for filtering ListView entities by status/enum values with real-time count displays using RadzenSelectBar with Templates.

**Implementation** (example from Leads ListView):

**Razor Template**:
```razor
@using Vanigam.CRM.Objects.Entities.Enums

<!-- Status Filter Bar -->
<RadzenRow class="mb-3">
    <RadzenColumn Size="12">
        <RadzenSelectBar @bind-Value="@SelectedStatus" TValue="LeadStatus?" Change="@OnStatusChange" class="w-100">
            <Items>
                <RadzenSelectBarItem Value="@((LeadStatus?)null)">
                    <Template>
                        <RadzenStack Orientation="Orientation.Vertical" AlignItems="AlignItems.Center" Gap="0.25rem" class="p-2">
                            <RadzenText TextStyle="TextStyle.Subtitle2" class="mb-0">@Localizer["All"]</RadzenText>
                            <RadzenBadge Text="@(StatusCounts.GetValueOrDefault(null, 0).ToString())"
                                         BadgeStyle="BadgeStyle.Info" />
                        </RadzenStack>
                    </Template>
                </RadzenSelectBarItem>
                @foreach (EntityStatus status in Enum.GetValues<EntityStatus>())
                {
                    <RadzenSelectBarItem Value="@status">
                        <Template>
                            <RadzenStack Orientation="Orientation.Vertical" AlignItems="AlignItems.Center" Gap="0.25rem" class="p-2">
                                <RadzenText TextStyle="TextStyle.Subtitle2" class="mb-0">@Localizer[status.ToString()]</RadzenText>
                                <RadzenBadge Text="@(StatusCounts.GetValueOrDefault(status, 0).ToString())"
                                             BadgeStyle="@GetStatusBadgeStyle(status)" />
                            </RadzenStack>
                        </Template>
                    </RadzenSelectBarItem>
                }
            </Items>
        </RadzenSelectBar>
    </RadzenColumn>
</RadzenRow>
```

**Code-behind Implementation**:
```csharp
using Vanigam.CRM.Objects.Entities.Enums;

public partial class EntityListView
{
    private EntityStatus? SelectedStatus = null;
    private Dictionary<EntityStatus?, int> StatusCounts = new();

    protected override string GetFilterString(LoadDataArgs args)
    {
        var filter = new ODataFilter<Entity>()
            .FilterByAnd(args.Filter);

        // Add status filter if selected
        if (SelectedStatus.HasValue)
        {
            filter = filter.FilterByAnd($"Status eq '{SelectedStatus.Value}'");
        }

        return filter
            .BeginGroup()
            // Add searchable properties
            .ContainsOr(u => u.Name, SearchString)
            .EndGroup()
            .Build();
    }

    private async Task LoadStatusCounts()
    {
        try
        {
            StatusCounts.Clear();

            // Get total count for "All"
            var allResult = await EntityApiService.Get(filter: GetBaseFilterString(), count: true, top: 0);
            StatusCounts[null] = allResult.Count;

            // Get counts for each status
            foreach (EntityStatus status in Enum.GetValues<EntityStatus>())
            {
                var statusFilter = GetBaseFilterString();
                if (!string.IsNullOrEmpty(statusFilter))
                    statusFilter += $" and Status eq '{status}'";
                else
                    statusFilter = $"Status eq '{status}'";

                var statusResult = await EntityApiService.Get(filter: statusFilter, count: true, top: 0);
                StatusCounts[status] = statusResult.Count;
            }
        }
        catch (Exception ex)
        {
            // If status counts fail, set to 0
            StatusCounts[null] = 0;
            foreach (EntityStatus status in Enum.GetValues<EntityStatus>())
            {
                StatusCounts[status] = 0;
            }
        }
    }

    private string GetBaseFilterString()
    {
        if (string.IsNullOrEmpty(SearchString))
            return string.Empty;

        return new ODataFilter<Entity>()
            .BeginGroup()
            .ContainsOr(u => u.Name, SearchString)
            // Add other searchable properties
            .EndGroup()
            .Build();
    }

    protected async Task OnStatusChange(EntityStatus? value)
    {
        SelectedStatus = value;
        await GridReload();
    }

    // Overloaded methods for nullable and non-nullable status types
    protected BadgeStyle GetStatusBadgeStyle(EntityStatus? status)
    {
        if (!status.HasValue)
            return BadgeStyle.Info; // For "All" option

        return GetStatusBadgeStyle(status.Value);
    }

    protected BadgeStyle GetStatusBadgeStyle(EntityStatus status)
    {
        return status switch
        {
            EntityStatus.Active => BadgeStyle.Success,
            EntityStatus.Inactive => BadgeStyle.Secondary,
            EntityStatus.Pending => BadgeStyle.Warning,
            EntityStatus.Cancelled => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }
}
```

**Localization Required**:
- Create `{EntityListView}.razor.en.resx` and `{EntityListView}.razor.ta.resx`
- Add entries for "All" and each enum value
- Example: `"New"`, `"Contacted"`, `"Qualified"`, `"Converted"`, `"Lost"`

**Key Features**:
1. **Real-time counts**: Shows current count for each status
2. **Visual feedback**: Selected status highlighted with primary color
3. **Click filtering**: Clicking a card filters the grid by that status
4. **"All" option**: Shows unfiltered results
5. **Status-specific badge colors**: Different colors per status type
6. **Responsive layout**: Uses RadzenColumn Size="2" for 6 cards per row

**Implementation Checklist**:
1. Add using statement for enum namespace
2. Add SelectedStatus and StatusCounts properties
3. Modify GetFilterString to include status filtering
4. Add LoadStatusCounts method (optimized with Summary API)
5. Add GetBaseFilterString method (search without status filter)
6. Add OnStatusChange event handler for RadzenSelectBar
7. Add overloaded GetStatusBadgeStyle methods (nullable and non-nullable)
8. Call LoadStatusCounts in GridLoadData method
9. Create RadzenSelectBar with Items and RadzenSelectBarItem using Templates
10. Create localization files for status values

#### Generic Summary API Pattern

**Overview**: To optimize performance, use the Generic Summary API that returns all status counts in a single request instead of multiple API calls.

**Implementation**:

**1. Summary DTOs** (`Objects/DTOs/SummaryDTOs.cs`):
```csharp
public class StatusSummaryResponse<TEnum> where TEnum : Enum
{
    public int TotalCount { get; set; }
    public Dictionary<TEnum, int> StatusCounts { get; set; } = new();

    [JsonIgnore]
    public Dictionary<TEnum?, int> StatusCountsNullable
    {
        get
        {
            var result = StatusCounts.ToDictionary(kv => (TEnum?)kv.Key, kv => kv.Value);
            result[null] = TotalCount;
            return result;
        }
    }
}

public class StatusSummaryRequest
{
    public string? SearchFilter { get; set; }
    public string? AdditionalFilter { get; set; }
}
```

**2. Generic Summary Service** (`Server/Services/SummaryService.cs`):
```csharp
public class SummaryService<TEntity, TEnum>(
    VanigamAccountingDbContext context,
    ICurrentUserService currentUserService,
    ILogger<SummaryService<TEntity, TEnum>> logger)
    where TEntity : BaseClass
    where TEnum : Enum
{
    public async Task<StatusSummaryResponse<TEnum>> GetStatusSummaryAsync(
        DbSet<TEntity> dbSet,
        Expression<Func<TEntity, TEnum>> statusProperty,
        string? searchFilter = null,
        string? additionalFilter = null)
    {
        // Single optimized query with GROUP BY
        var query = dbSet.AsQueryable()
            .Where(e => e.IsNotDeleted);

        // Apply tenant and search filters
        // Get total count and status counts in efficient queries

        return new StatusSummaryResponse<TEnum>
        {
            TotalCount = totalCount,
            StatusCounts = statusCounts
        };
    }
}
```

**3. Enhanced OData Controller** (add to existing controller):
```csharp
public class LeadsController(
    // existing parameters
    SummaryService<Lead, LeadStatus> summaryService,
    ILogger<LeadsController> logger)
    : BaseODataServiceController<Lead, LeadService>(...)
{
    [HttpPost("status-summary")]
    [Route("status-summary")]
    public async Task<ActionResult<StatusSummaryResponse<LeadStatus>>> GetStatusSummary(
        [FromBody] StatusSummaryRequest request)
    {
        var result = await summaryService.GetStatusSummaryAsync(
            Context.Leads,
            lead => lead.Status,
            request.SearchFilter,
            request.AdditionalFilter);

        return Ok(result);
    }
}
```

**4. API Service Extension** (add to existing API service):
```csharp
public class LeadApiService : BaseApiService<Lead>
{
    public async Task<StatusSummaryResponse<LeadStatus>> GetStatusSummaryAsync(
        StatusSummaryRequest request)
    {
        var response = await HttpClient.PostAsJsonAsync(
            $"odata/VanigamAccountingService/Leads/status-summary",
            request);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StatusSummaryResponse<LeadStatus>>(json, JsonOptions)
               ?? new StatusSummaryResponse<LeadStatus>();
    }
}
```

**5. Optimized LoadStatusCounts Method**:
```csharp
private async Task LoadStatusCounts()
{
    try
    {
        var request = new StatusSummaryRequest
        {
            SearchFilter = GetBaseFilterString()
        };

        var summary = await LeadApiService.GetStatusSummaryAsync(request);
        StatusCounts = summary.StatusCountsNullable;
    }
    catch (Exception ex)
    {
        // Fallback to zero counts
        StatusCounts.Clear();
        StatusCounts[null] = 0;
        foreach (LeadStatus status in Enum.GetValues<LeadStatus>())
        {
            StatusCounts[status] = 0;
        }
    }
}
```

**6. Service Registration** (`Program.cs`):
```csharp
builder.Services.AddScoped(typeof(SummaryService<,>));
```

**Performance Benefits**:
- **Reduces API calls**: From 6 individual requests to 1 single request
- **Database efficiency**: Single GROUP BY query instead of multiple COUNT queries
- **Atomic consistency**: All counts retrieved in same transaction
- **Network optimization**: Reduced HTTP overhead and faster UI response

**Reusability**: This pattern can be applied to any entity with status enums (Opportunities, Customers, etc.) by:
1. Adding SummaryService<EntityType, StatusEnum> to controller constructor
2. Adding status-summary endpoint to existing OData controller
3. Adding GetStatusSummaryAsync method to entity's API service
4. Using optimized LoadStatusCounts pattern in ListView

When adding new entities:
1. Create entity class inheriting from `BaseClass` or `NamedClass`/`CodedClass`
2. Add proper EF Core data annotations (`[StringLength]`, `[ForeignKey]`, etc.)
3. Add `DbSet<T>` property to `VanigamAccountingDbContext`
4. Register entity in `ODataExtensions.InitOData()`
5. Create corresponding service inheriting from `BaseService<T>`
6. Create API service inheriting from `BaseApiService<T>` if needed
7. Create SQL script for database schema changes (no EF migrations used)