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

See `EntityAndServicePatterns.md` for comprehensive patterns and templates for implementing entities, services, controllers, and UI components.

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
- **Entity conversions**: Conversion functionality (e.g., Lead to Opportunity, Opportunity to Customer) must be implemented within the primary entity's service, controller, and API service rather than separate conversion services. This maintains architectural consistency and follows the established patterns.

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

See `StatusFilterBarPattern.md` for detailed implementation guide.

### Webhook Integration

**Overview**: The application supports webhook integration for automatic lead capture from social media platforms.

#### Supported Platforms
- **Facebook Lead Ads**: Captures leads from Facebook Lead Generation campaigns
- **WhatsApp Business**: Processes incoming messages as potential leads
- **Instagram**: Handles direct messages for lead capture
- **Generic Social Media**: Flexible endpoint for other platforms

#### Webhook Endpoints

**Facebook Webhooks**:
- `GET /api/webhooks/leads/facebook/verify` - Webhook verification endpoint
- `POST /api/webhooks/leads/facebook` - Lead data processing endpoint

**WhatsApp Webhooks**:
- `GET /api/webhooks/leads/whatsapp/verify` - Webhook verification endpoint
- `POST /api/webhooks/leads/whatsapp` - Message processing endpoint

**Instagram Webhooks**:
- `GET /api/webhooks/leads/instagram/verify` - Webhook verification endpoint
- `POST /api/webhooks/leads/instagram` - Message processing endpoint

**Generic Webhook**:
- `POST /api/webhooks/leads/generic` - General-purpose lead capture endpoint

**Health Check**:
- `GET /api/webhooks/leads/health` - Webhook service health status

#### Configuration

**appsettings.json**:
```json
{
  "Webhook": {
    "Facebook": {
      "VerifyToken": "your_facebook_verify_token_here",
      "AppSecret": "your_facebook_app_secret_here",
      "AccessToken": "your_facebook_access_token_here"
    },
    "WhatsApp": {
      "VerifyToken": "your_whatsapp_verify_token_here",
      "AppSecret": "your_whatsapp_app_secret_here"
    },
    "Instagram": {
      "VerifyToken": "your_instagram_verify_token_here",
      "AppSecret": "your_instagram_app_secret_here"
    }
  }
}
```

#### Service Components

**WebhookLeadService** (`Server/Services/WebhookLeadService.cs`):
- Lead creation from webhook data
- Platform-specific data mapping
- Signature validation for security
- Activity tracking for audit trail

**LeadsWebhookController** (`Server/Controllers/WebhookController/LeadsWebhookController.cs`):
- Webhook endpoint handling
- Platform verification support
- Secure payload processing
- Error handling and logging

**Webhook DTOs** (`Objects/DTOs/WebhookDTOs.cs`):
- Platform-specific data models
- Generic webhook structures
- Response models

#### Security Features

1. **Signature Validation**: HMAC-SHA256 signature verification for Facebook/Meta platforms
2. **Verify Token**: Token-based webhook verification during setup
3. **HTTPS Only**: All webhook endpoints require HTTPS in production
4. **Tenant Isolation**: Automatic tenant assignment based on authentication context

#### Lead Processing Flow

1. **Webhook Reception**: Platform sends webhook payload to appropriate endpoint
2. **Signature Validation**: Verify webhook authenticity using platform-specific secrets
3. **Data Mapping**: Transform platform data to internal Lead model
4. **Lead Creation**: Create new Lead entity with proper tenant assignment
5. **Activity Logging**: Record lead creation activity for audit trail
6. **Response**: Return success/error response to platform

#### Platform-Specific Features

**Facebook Lead Ads**:
- Automatic lead detail fetching using Graph API
- Form field mapping to Lead properties
- Campaign and ad attribution tracking

**WhatsApp Business**:
- Contact profile information extraction
- Message content as lead description
- Phone number as primary contact method

**Instagram**:
- Direct message processing
- Sender ID tracking
- Message content analysis

**Generic Platform**:
- Flexible JSON payload structure
- Custom field mapping support
- Multi-platform compatibility

#### Error Handling

- **Invalid Signatures**: Return 401 Unauthorized
- **Missing Configuration**: Log errors and return appropriate status
- **Processing Failures**: Continue processing other entries, log individual failures
- **Database Errors**: Transaction rollback with detailed logging

#### Monitoring and Logging

- **Health Check Endpoint**: `/api/webhooks/leads/health`
- **Structured Logging**: All webhook activities logged with correlation IDs
- **Error Tracking**: Failed webhook processing logged with full context
- **Success Metrics**: Lead creation tracking per platform

When adding new entities:
1. Create entity class inheriting from `BaseClass` or `NamedClass`/`CodedClass`
2. Add proper EF Core data annotations (`[StringLength]`, `[ForeignKey]`, etc.)
3. Add `DbSet<T>` property to `VanigamAccountingDbContext`
4. Register entity in `ODataExtensions.InitOData()`
5. Create corresponding service inheriting from `BaseService<T>`
6. Create API service inheriting from `BaseApiService<T>` if needed
7. Create SQL script for database schema changes (no EF migrations used)