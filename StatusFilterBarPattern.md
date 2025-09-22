# Status Filter Bar Pattern

**Overview**: Status filter bar provides a visual interface for filtering ListView entities by status/enum values with real-time count displays using RadzenSelectBar with Templates.

## Implementation

**Razor Template** (Optimized Pattern - Use This):
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

**Code-behind Implementation** (Optimized Pattern - Use This):
```csharp
using Vanigam.CRM.Objects.Entities.Enums;
using Vanigam.CRM.Objects.DTOs;

public partial class EntityListView
{
    private EntityStatus? SelectedStatus = null;
    private Dictionary<EntityStatus, int> StatusCounts = new();
    private int TotalCount = 0;

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
1. Add using statement for enum namespace
2. Add SelectedStatus and StatusCounts properties + TotalCount property
3. Modify GetFilterString to include status filtering
4. Add LoadStatusCounts method (optimized with Summary API)
5. Add GetBaseFilterString method (search without status filter)
6. Add OnStatusChange event handler for RadzenSelectBar
7. Add overloaded GetStatusBadgeStyle methods (nullable and non-nullable)
8. Add GetStatusCount method for safe count retrieval
9. Call LoadStatusCounts in GridLoadData method
10. **Place RadzenSelectBar inside `<RadioButtons>` section of ListPageTitleComponent**
11. Create RadzenSelectBar with Items and RadzenSelectBarItem using Templates
12. Create localization files for status values
13. Add GetStatusSummaryAsync method to entity's API service
14. Add status-summary endpoint to entity's OData controller

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