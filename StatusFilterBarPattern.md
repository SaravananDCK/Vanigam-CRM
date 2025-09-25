# Status Filter Bar Pattern

**Overview**: Status filter bar provides a visual interface for filtering ListView entities by status/enum values with real-time count displays using RadzenSelectBar with Templates.

## Implementation

**Razor Template - Modern RadzenTabs with FilterTabTemplate** (Recommended - Latest Version):
```razor
<RadzenStack>
    <ListPageTitleComponent TitleText=@Localizer["EntityPlural"]
                            AddButtonClick=@AddButtonClick
                            SearchButtonClick=@Search
                            ShowAdd=@(!IsEmbeddedMode)>
        <RadioButtons>
            @* Status Filter Bar using RadzenTabs with FilterTabTemplate *@
            <RadzenTabs RenderMode="TabRenderMode.Client" SelectedIndexChanged="OnStatusTabChange"
                        TabPosition="TabPosition.Top" class="modern-tabs">
                <Tabs>
                    <RadzenTabsItem>
                        <Template>
                            <FilterTabTemplate Text="@Localizer["All"]"
                                             Count="@(TotalCount.ToString() ?? "0")"
                                             Icon="@FadIcon("fa-list")"
                                             Style="@BadgeStyle.Primary" />
                        </Template>
                    </RadzenTabsItem>
                    @foreach (EntityStatus status in Enum.GetValues<EntityStatus>())
                    {
                        <RadzenTabsItem>
                            <Template>
                                <FilterTabTemplate Text="@Localizer[status.ToString()]"
                                                 Count="@(GetStatusCount(status).ToString())"
                                                 Icon="@FadIcon(GetIcon(status))"
                                                 Style="@GetStatusBadgeStyle(status)" />
                            </Template>
                        </RadzenTabsItem>
                    }
                </Tabs>
            </RadzenTabs>
        </RadioButtons>
    </ListPageTitleComponent>

    <!-- Rest of your ListView content -->
</RadzenStack>

<style>
    :root {
        .rz-tabview.rz-tabview-top { flex-direction: row !important; }
    }
</style>
```

**Alternative Pattern - RadzenSelectBar** (Still Supported):
```razor
<RadzenStack>
    <ListPageTitleComponent TitleText=@Localizer["EntityPlural"]
                            AddButtonClick=@AddButtonClick
                            SearchButtonClick=@Search
                            ShowAdd=@(!IsEmbeddedMode)>
        <RadioButtons>
            @* Status Filter Bar - Only show in non-embedded mode *@
            @if (!IsEmbeddedMode)
            {
                <RadzenRow class="mb-3" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.Center">
                    <RadzenColumn Size="12">
                        <RadzenSelectBar @bind-Value="@SelectedStatus" TValue="EntityStatus?" Change="@OnStatusChange" class="w-100">
                            <Items>
                                <RadzenSelectBarItem Value="@(null)">
                                    <Template>
                                        <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.25rem" class="p-2">
                                            <RadzenText TextStyle="TextStyle.Subtitle2" class="mb-0">@Localizer["All"]</RadzenText>
                                            <RadzenBadge Text="@(TotalCount.ToString())"
                                                         BadgeStyle="BadgeStyle.Info" />
                                        </RadzenStack>
                                    </Template>
                                </RadzenSelectBarItem>
                                @foreach (EntityStatus status in Enum.GetValues<EntityStatus>())
                                {
                                    <RadzenSelectBarItem Value="@status">
                                        <Template>
                                            <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.25rem" class="p-2">
                                                <RadzenText TextStyle="TextStyle.Subtitle2" class="mb-0">@Localizer[status.ToString()]</RadzenText>
                                                <RadzenBadge Text="@(GetStatusCount(status).ToString())"
                                                             BadgeStyle="@GetStatusBadgeStyle(status)" />
                                            </RadzenStack>
                                        </Template>
                                    </RadzenSelectBarItem>
                                }
                            </Items>
                        </RadzenSelectBar>
                    </RadzenColumn>
                </RadzenRow>
            }
        </RadioButtons>
    </ListPageTitleComponent>

    <!-- Rest of your ListView content -->
</RadzenStack>
```

**Key Template Changes for Optimization**:
- Use `@(null)` instead of `@((EntityStatus?)null)` for "All" option
- Use `TotalCount` property for "All" badge count
- Use `GetStatusCount(status)` method for individual status counts
- Use `Orientation.Horizontal` for better layout
- **IMPORTANT**: Place the status filter bar inside `<RadioButtons>` section of `ListPageTitleComponent`

## Placement Guidelines

### ✅ Correct Placement - Inside RadioButtons
The status filter bar must be placed within the `<RadioButtons>` section of the `ListPageTitleComponent`. This ensures proper layout integration and consistent styling across all ListView pages.

```razor
<ListPageTitleComponent TitleText=@Localizer["EntityPlural"]
                        AddButtonClick=@AddButtonClick
                        SearchButtonClick=@Search>
    <RadioButtons>
        <!-- Status Filter Bar goes here -->
        <RadzenRow class="mb-3" AlignItems="AlignItems.Center" JustifyContent="JustifyContent.Center">
            <!-- RadzenSelectBar implementation -->
        </RadzenRow>
    </RadioButtons>
</ListPageTitleComponent>
```

### ❌ Incorrect Placement - Outside Component
Do not place the status filter bar outside the `ListPageTitleComponent` as this breaks the layout consistency:

```razor
<!-- DON'T DO THIS -->
<ListPageTitleComponent ... />
<RadzenRow class="mb-3">
    <!-- Status filter here breaks layout -->
</RadzenRow>
```

**Code-behind Implementation - Modern RadzenTabs Pattern** (Recommended):

### Option 1: Separate Code-Behind File (Recommended)
Create a separate `.razor.cs` file (e.g., `Opportunities.razor.cs`):

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Opportunities
    {
        [Parameter] public Guid? LeadId { get; set; }
        [Parameter] public bool IsEmbeddedMode { get; set; } = false;
        [Parameter] public string? EmbeddedTitle { get; set; }

        private OpportunityStage? SelectedStage = null;
        private Dictionary<OpportunityStage, int> StageCounts = new();
        private int TotalCount = 0;
        private int SelectedTabIndex = 0;

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<Opportunity>()
                .FilterByAnd(args.Filter);

            // Filter by parent Lead if in embedded mode
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.LeadId == LeadId.Value);
            }

            // Add stage filter if selected
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
            var request = new StatusSummaryRequest
            {
                SearchFilter = GetBaseFilterString()
            };

            var summary = await EntityApiService.GetStatusSummaryAsync(request);
            TotalCount = summary.TotalCount;
            StatusCounts = summary.StatusCounts.ToDictionary(kv => (EntityStatus)kv.Key, kv => kv.Value);
        }
        catch (Exception ex)
        {
            // Fallback to zero counts if API call fails
            StatusCounts.Clear();
            TotalCount = 0;
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

    // RadzenTabs-specific methods
    protected async Task OnStatusTabChange(int tabIndex)
    {
        SelectedTabIndex = tabIndex;

        if (tabIndex == 0)
        {
            // "All" tab selected
            SelectedStatus = null;
        }
        else
        {
            // Status tab selected (tabIndex - 1 because first tab is "All")
            var statusValues = Enum.GetValues<EntityStatus>();
            if (tabIndex - 1 < statusValues.Length)
            {
                SelectedStatus = statusValues[tabIndex - 1];
            }
        }

        await GridReload();
    }

    protected string GetIcon(EntityStatus status)
    {
        return status switch
        {
            EntityStatus.New => "fa-plus-circle",
            EntityStatus.Active => "fa-play-circle",
            EntityStatus.InProgress => "fa-clock",
            EntityStatus.Completed => "fa-check-circle",
            EntityStatus.Cancelled => "fa-times-circle",
            EntityStatus.Pending => "fa-pause-circle",
            _ => "fa-question-circle"
        };
    }

    protected string GetIconColor(EntityStatus status)
    {
        return status switch
        {
            EntityStatus.New => "var(--rz-info)",
            EntityStatus.Active => "var(--rz-success)",
            EntityStatus.InProgress => "var(--rz-warning)",
            EntityStatus.Completed => "var(--rz-success)",
            EntityStatus.Cancelled => "var(--rz-danger)",
            EntityStatus.Pending => "var(--rz-secondary)",
            _ => "var(--rz-text-color)"
        };
    }

    protected string FadIcon(string iconClass)
    {
        return $"fad {iconClass}";
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

    protected int GetStatusCount(EntityStatus status)
    {
        if (StatusCounts == null)
            return 0;

        return StatusCounts.TryGetValue(status, out var count) ? count : 0;
    }
}
```

### Code-Behind File Architecture

**MANDATORY**: The project **requires** separate `.razor.cs` code-behind files for all ListView components implementing status filter bars. This is not optional - it is an architectural requirement.

**File Structure:**
- `Opportunities.razor` - Contains the Razor markup and UI components
- `Opportunities.razor.cs` - Contains the C# logic, event handlers, and data operations

**Code-Behind Pattern Benefits:**
- **Separation of Concerns**: UI markup separated from business logic
- **Better Maintainability**: Easier to manage complex ListView logic
- **Improved Readability**: Clean separation between presentation and code
- **Enhanced Tooling**: Better IntelliSense and debugging support
- **Team Collaboration**: Easier for different developers to work on UI vs logic
- **Architectural Consistency**: Ensures uniform code organization across all ListView pages

**Example Implementation:**

**File: `Opportunities.razor.cs`**
```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Opportunities
    {
        // All the C# logic, properties, and methods
        // (Complete implementation shown in previous sections)
    }
}
```

**File: `Opportunities.razor`**
```razor
@page "/opportunities"
@using Vanigam.CRM.Objects.Entities
@inherits Vanigam.CRM.Client.Components.BaseListView<Opportunity, Opportunities>
@inject OpportunityApiService OpportunityApiService

<!-- All the Razor markup -->
<!-- (Complete markup shown in previous sections) -->
```

### ❌ PROHIBITED: Inline @code Section

**DO NOT USE** inline `@code` sections in ListView components with status filter bars. This approach is prohibited for the following reasons:

```razor
<!-- PROHIBITED PATTERN - DO NOT USE -->
@code {
    // This is not allowed in ListView components
    // All C# logic must be in separate .razor.cs files
}
```

**Why Inline @code is Prohibited:**
- **Violates Architecture**: Breaks established code-behind pattern
- **Poor Maintainability**: Complex ListView logic becomes unmanageable in single files
- **Inconsistent**: Creates inconsistency across the codebase
- **Team Issues**: Makes collaborative development more difficult
- **Debugging Difficulties**: Harder to debug and test when mixed with markup

**MANDATORY**: Use separate `.razor.cs` files for all ListView components implementing status filter bars.

**Key Code Changes for Optimization**:
- Change `Dictionary<EntityStatus?, int>` to `Dictionary<EntityStatus, int>`
- Add `int TotalCount` property for "All" badge
- Replace multiple API calls with single `GetStatusSummaryAsync()` call
- Add `GetStatusCount(status)` method for safe count retrieval
- Add `using Vanigam.CRM.Objects.DTOs;` for `StatusSummaryRequest`

## Localization Required
- Create `{EntityListView}.razor.en.resx` and `{EntityListView}.razor.ta.resx`
- Add entries for "All" and each enum value
- Example: `"New"`, `"Contacted"`, `"Qualified"`, `"Converted"`, `"Lost"`

## Key Features
1. **Real-time counts**: Shows current count for each status
2. **Visual feedback**: Selected status highlighted with primary color
3. **Click filtering**: Clicking a card filters the grid by that status
4. **"All" option**: Shows unfiltered results
5. **Status-specific badge colors**: Different colors per status type
6. **Responsive layout**: Uses RadzenColumn Size="2" for 6 cards per row

## Implementation Checklist

### RadzenTabs Pattern (Recommended)

**Prerequisites:**
1. **✅ MANDATORY**: Create separate `.razor.cs` code-behind file
2. **✅ MANDATORY**: Move all C# logic to code-behind file
3. **✅ MANDATORY**: Keep only Razor markup in `.razor` file

**Implementation Steps:**
4. Add using statements for enum namespace in code-behind file
5. Add SelectedStatus, StatusCounts, TotalCount properties in code-behind
6. Add [Parameter] properties for embedded mode support in code-behind
7. Modify GetFilterString to include status filtering in code-behind
8. Add LoadStatusCounts method (optimized with Summary API) in code-behind
9. Add GetBaseFilterString method (search without status filter) in code-behind
10. Add OnStatusTabChange event handler for RadzenTabs in code-behind
11. Add GetIcon and GetIconColor methods for status-specific icons in code-behind
12. Add FadIcon helper method for FontAwesome Duotone icons in code-behind
13. Add overloaded GetStatusBadgeStyle methods (nullable and non-nullable) in code-behind
14. Add GetStatusCount method for safe count retrieval in code-behind
15. Call LoadStatusCounts in GridLoadData method in code-behind
16. **Place RadzenTabs inside `<RadioButtons>` section of ListPageTitleComponent**
17. Create RadzenTabs with FilterTabTemplate components in `.razor` file
18. Add minimal CSS styles in `.razor` file
19. Create localization files for status values
20. Add GetStatusSummaryAsync method to entity's API service
21. Add status-summary endpoint to entity's OData controller

### RadzenSelectBar Pattern (Alternative - Legacy)

**Prerequisites:**
1. **✅ MANDATORY**: Create separate `.razor.cs` code-behind file
2. **✅ MANDATORY**: Move all C# logic to code-behind file
3. **✅ MANDATORY**: Keep only Razor markup in `.razor` file

**Implementation Steps:**
4. Add using statement for enum namespace in code-behind file
5. Add SelectedStatus and StatusCounts properties + TotalCount property in code-behind
6. Add [Parameter] properties for embedded mode support in code-behind
7. Modify GetFilterString to include status filtering in code-behind
8. Add LoadStatusCounts method (optimized with Summary API) in code-behind
9. Add GetBaseFilterString method (search without status filter) in code-behind
10. Add OnStatusChange event handler for RadzenSelectBar in code-behind
11. Add overloaded GetStatusBadgeStyle methods (nullable and non-nullable) in code-behind
12. Add GetStatusCount method for safe count retrieval in code-behind
13. Call LoadStatusCounts in GridLoadData method in code-behind
14. **Place RadzenSelectBar inside `<RadioButtons>` section of ListPageTitleComponent**
15. Create RadzenSelectBar with Items and RadzenSelectBarItem using Templates in `.razor` file
16. Create localization files for status values
17. Add GetStatusSummaryAsync method to entity's API service
18. Add status-summary endpoint to entity's OData controller

## Backend Requirements for Optimization

### Required API Service Method

Add to your entity's API service (e.g., `LeadApiService.cs`, `OpportunityApiService.cs`):

```csharp
using System.Text.Json;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities.Enums;
using System.Net.Http.Json;

public async Task<StatusSummaryResponse<EntityStatus>> GetStatusSummaryAsync(StatusSummaryRequest request)
{
    try
    {
        var response = await HttpClient.PostAsJsonAsync(
            $"odata/VanigamAccountingService/EntityPlural/status-summary",
            request);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<StatusSummaryResponse<EntityStatus>>(json, GetJsonSerializerOptions())
               ?? new StatusSummaryResponse<EntityStatus>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting entity status summary: {ex.Message}");
        return new StatusSummaryResponse<EntityStatus>
        {
            TotalCount = 0,
            StatusCounts = new Dictionary<EntityStatus, int>()
        };
    }
}
```

### Required Controller Endpoint

Add to your entity's OData controller:

```csharp
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities.Enums;

public class EntityController(
    // existing parameters
    SummaryService<Entity, EntityStatus> summaryService,
    ILogger<EntityController> logger)
    : BaseODataServiceController<Entity, EntityService>(...)
{
    [HttpPost("status-summary")]
    [Route("status-summary")]
    public async Task<ActionResult<StatusSummaryResponse<EntityStatus>>> GetStatusSummary(
        [FromBody] StatusSummaryRequest request)
    {
        try
        {
            var result = await summaryService.GetStatusSummaryAsync(
                Context.EntityPlural,
                entity => entity.Status,
                request.SearchFilter,
                request.AdditionalFilter);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting entity status summary");
            return BadRequest(new { Error = "Failed to retrieve status summary" });
        }
    }
}
```

## Generic Summary API Pattern

**Overview**: To optimize performance, use the Generic Summary API that returns all status counts in a single request instead of multiple API calls.

### Implementation

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

### Performance Benefits
- **Reduces API calls**: From 6 individual requests to 1 single request
- **Database efficiency**: Single GROUP BY query instead of multiple COUNT queries
- **Atomic consistency**: All counts retrieved in same transaction
- **Network optimization**: Reduced HTTP overhead and faster UI response

### Reusability
This pattern can be applied to any entity with status enums (Opportunities, Customers, etc.) by:
1. Adding SummaryService<EntityType, StatusEnum> to controller constructor
2. Adding status-summary endpoint to existing OData controller
3. Adding GetStatusSummaryAsync method to entity's API service
4. Using optimized LoadStatusCounts pattern in ListView

## Pattern Comparison

### RadzenTabs Pattern (Recommended)
**Advantages:**
- **Visual Appeal**: More modern, tab-based interface with icons and badges
- **Better UX**: Clear visual separation between status categories
- **Icon Support**: Status-specific icons provide immediate visual context
- **Flexible Styling**: Easy to customize appearance with CSS
- **Mobile Friendly**: Better responsive behavior on mobile devices
- **FontAwesome Integration**: Supports FontAwesome Duotone icons for richer visuals

**Use Cases:**
- Primary ListViews with prominent status filtering needs
- When visual differentiation between statuses is important
- Applications with modern UI requirements
- Mobile-responsive applications

### RadzenSelectBar Pattern (Alternative)
**Advantages:**
- **Compact Layout**: Takes less vertical space
- **Consistent Behavior**: Standard Radzen component behavior
- **Quick Implementation**: Easier to implement initially
- **Horizontal Layout**: Better for limited vertical space

**Use Cases:**
- Secondary ListViews or embedded views
- When space is constrained
- Quick prototyping or simple filtering needs
- Applications prioritizing consistency over visual appeal

## Migration Guide

To migrate from RadzenSelectBar to RadzenTabs pattern:

1. **Replace the markup** with RadzenTabs structure
2. **Add SelectedTabIndex property** and OnStatusTabChange method
3. **Add icon methods** (GetIcon, GetIconColor, FadIcon)
4. **Add CSS styles** for tab-content and tab-icon classes
5. **Update event handling** from OnStatusChange to OnStatusTabChange
6. **Test functionality** to ensure proper tab switching and filtering

## FilterTabTemplate Component Pattern

**Overview**: The FilterTabTemplate component provides a reusable, clean approach for status filter tabs, eliminating code duplication and providing consistent styling across all ListView pages.

### Component Structure

**FilterTabTemplate.razor**:
```razor
<RadzenStack Orientation="Orientation.Vertical" Gap="0">
    <RadzenStack Orientation="Orientation.Horizontal" Gap="5">
        <span class="tab-label">@Text</span>
        <RadzenIcon Icon="@Icon" IconColor="@GetIconColor(Style)" />
    </RadzenStack>
    <ModernRoundBadge Text="@(Count)" BadgeStyle="@Style" />
</RadzenStack>

@code {
    [Parameter] public string Text { get; set; }
    [Parameter] public string Count { get; set; }
    [Parameter] public string Icon { get; set; }
    [Parameter] public BadgeStyle Style { get; set; }

    protected string GetIconColor(BadgeStyle style)
    {
        return style switch
        {
            BadgeStyle.Info => "var(--rz-info)",
            BadgeStyle.Primary => "var(--rz-primary)",
            BadgeStyle.Secondary => "var(--rz-secondary)",
            BadgeStyle.Warning => "var(--rz-warning)",
            BadgeStyle.Success => "var(--rz-success)",
            BadgeStyle.Danger => "var(--rz-danger)",
            _ => "var(--rz-text-color)"
        };
    }
}
```

### Usage in ListView Pages

**Before (Inline Template)**:
```razor
<RadzenTabsItem Icon="@FadIcon(GetIcon(status))" IconColor="@GetIconColor(status)">
    <Template>
        <div class="tab-content">
            <span class="tab-label">@Localizer[status.ToString()]</span>
            <ModernRoundBadge Text="@(GetStatusCount(status).ToString())" BadgeStyle="@GetStatusBadgeStyle(status)" />
        </div>
    </Template>
</RadzenTabsItem>
```

**After (FilterTabTemplate)**:
```razor
<RadzenTabsItem>
    <Template>
        <FilterTabTemplate Text="@Localizer[status.ToString()]"
                         Count="@(GetStatusCount(status).ToString())"
                         Icon="@FadIcon(GetIcon(status))"
                         Style="@GetStatusBadgeStyle(status)" />
    </Template>
</RadzenTabsItem>
```

### Benefits of FilterTabTemplate Pattern

1. **Code Reusability**: Single component used across all ListView pages
2. **Consistent Styling**: Ensures uniform appearance across the application
3. **Maintainability**: Changes to tab styling only require updating one component
4. **Cleaner Markup**: Reduces code complexity in ListView pages
5. **Better Organization**: Separates presentation logic from business logic
6. **Reduced CSS**: Eliminates need for custom tab-content CSS in each page
7. **Type Safety**: Strongly-typed parameters prevent runtime errors
8. **Testability**: Component can be unit tested independently

### Migration to FilterTabTemplate

**Steps to migrate existing RadzenTabs implementation**:

1. **Remove custom CSS**: Delete tab-content and tab-icon styles from ListView pages
2. **Replace inline templates**: Use FilterTabTemplate component instead of div structures
3. **Remove IconColor attributes**: FilterTabTemplate handles icon coloring internally
4. **Simplify Template sections**: Remove complex div and span structures
5. **Update all ListView pages**: Apply the pattern consistently across the application

**Example Migration**:

```razor
<!-- OLD: Complex inline template -->
<RadzenTabsItem Icon="@FadIcon(GetIcon(status))" IconColor="@GetIconColor(status)">
    <Template>
        <div class="tab-content">
            <span class="tab-label">@Localizer[status.ToString()]</span>
            <ModernRoundBadge Text="@(GetStatusCount(status).ToString())" BadgeStyle="@GetStatusBadgeStyle(status)" />
        </div>
    </Template>
</RadzenTabsItem>

<!-- NEW: Clean component usage -->
<RadzenTabsItem>
    <Template>
        <FilterTabTemplate Text="@Localizer[status.ToString()]"
                         Count="@(GetStatusCount(status).ToString())"
                         Icon="@FadIcon(GetIcon(status))"
                         Style="@GetStatusBadgeStyle(status)" />
    </Template>
</RadzenTabsItem>
```

### Component Registration

Ensure the FilterTabTemplate component is available in your ListView pages by adding the appropriate using statement or registering it in `_Imports.razor`:

```razor
@using Vanigam.CRM.Client.Components
```

## Best Practices

1. **✅ MANDATORY Code-Behind**: Always use separate `.razor.cs` files for ListView components
2. **✅ MANDATORY Markup Separation**: Keep only Razor markup in `.razor` files
3. **Consistent Icons**: Use meaningful, consistent icons across similar entity types
4. **Color Coding**: Align icon colors with badge colors for visual consistency
5. **Responsive Design**: Test on mobile devices and adjust CSS accordingly
6. **Performance**: Implement status count caching for better performance
7. **Accessibility**: Ensure proper ARIA labels and keyboard navigation
8. **Localization**: Provide translations for all status labels and "All" option
9. **Component Reuse**: Use FilterTabTemplate for all status filter implementations
10. **Minimal CSS**: Let FilterTabTemplate handle styling; avoid custom CSS in ListView pages
11. **Architecture Consistency**: Follow established patterns across all ListView implementations