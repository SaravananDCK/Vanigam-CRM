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
    public partial class Activities
    {
        [Parameter] public Guid? LeadId { get; set; }
        [Parameter] public Guid? OpportunityId { get; set; }
        [Parameter] public bool IsEmbeddedMode { get; set; } = false;
        [Parameter] public string? EmbeddedTitle { get; set; }

        private ActivityStatus? SelectedStatus = null;
        private Dictionary<ActivityStatus, int> StatusCounts = new();
        private int TotalCount = 0;
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await ActivityApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;

                // Load status counts only in non-embedded mode
                if (!IsEmbeddedMode)
                {
                    await LoadStatusCounts();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<Activity>()
                .FilterByAnd(args.Filter);

            // Filter by parent Lead or Opportunity if in embedded mode
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.LeadId == LeadId.Value);
            }
            else if (IsEmbeddedMode && OpportunityId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.OpportunityId == OpportunityId.Value);
            }

            // Add status filter if selected
            if (SelectedStatus.HasValue)
            {
                filter = filter.FilterByAnd($"Status eq '{SelectedStatus.Value}'");
            }

            filter.BeginGroup()
                .ContainsOr(u => u.Subject, SearchString)
                .ContainsOr(u => u.Description, SearchString)
                .ContainsOr(u => u.Notes, SearchString)
                .EndGroup();

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                parameters.Add("LeadId", LeadId.Value);
            }
            else if (IsEmbeddedMode && OpportunityId.HasValue)
            {
                parameters.Add("OpportunityId", OpportunityId.Value);
            }

            await DialogService.OpenDialogAsync<EditActivity>(Localizer["AddActivity"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Activity> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Activity activity)
        {
            await DialogService.OpenDialogAsync<EditActivity>(Localizer["EditActivity"], new Dictionary<string, object> { { "Oid", activity.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Activity activity)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await ActivityApiService.Delete(oid:activity.Oid);

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

        private async Task LoadStatusCounts()
        {
            try
            {
                var request = new StatusSummaryRequest
                {
                    SearchFilter = GetBaseFilterString()
                };

                var summary = await ActivityApiService.GetStatusSummaryAsync(request);
                TotalCount = summary.TotalCount;
                StatusCounts = summary.StatusCounts.ToDictionary(kv => (ActivityStatus)kv.Key, kv => kv.Value);
            }
            catch (Exception ex)
            {
                // Fallback to zero counts if API call fails
                StatusCounts.Clear();
                TotalCount = 0;
                foreach (ActivityStatus status in Enum.GetValues<ActivityStatus>())
                {
                    StatusCounts[status] = 0;
                }
            }
        }

        private string GetBaseFilterString()
        {
            var filter = new ODataFilter<Activity>();

            // Filter by parent Lead or Opportunity if in embedded mode
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.LeadId == LeadId.Value);
            }
            else if (IsEmbeddedMode && OpportunityId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.OpportunityId == OpportunityId.Value);
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.Subject, SearchString)
                    .ContainsOr(u => u.Description, SearchString)
                    .ContainsOr(u => u.Notes, SearchString)
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task OnStatusChange(ActivityStatus? value)
        {
            SelectedStatus = value;
            await GridReload();
        }

        // Overloaded methods for nullable and non-nullable status types
        protected BadgeStyle GetStatusBadgeStyle(ActivityStatus? status)
        {
            if (!status.HasValue)
                return BadgeStyle.Info; // For "All" option

            return GetStatusBadgeStyle(status.Value);
        }

        protected BadgeStyle GetStatusBadgeStyle(ActivityStatus status)
        {
            return status switch
            {
                ActivityStatus.NotStarted => BadgeStyle.Light,
                ActivityStatus.InProgress => BadgeStyle.Warning,
                ActivityStatus.Completed => BadgeStyle.Success,
                ActivityStatus.Cancelled => BadgeStyle.Danger,
                ActivityStatus.Pending => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }

        protected int GetStatusCount(ActivityStatus status)
        {
            if (StatusCounts == null)
                return 0;

            return StatusCounts.TryGetValue(status, out var count) ? count : 0;
        }

        // RadzenTabs-specific methods
        protected async Task OnStatusTabChange(int tabIndex)
        {
            if (tabIndex == 0)
            {
                // "All" tab selected
                SelectedStatus = null;
            }
            else
            {
                // Status tab selected (tabIndex - 1 because first tab is "All")
                var statusValues = Enum.GetValues<ActivityStatus>();
                if (tabIndex - 1 < statusValues.Length)
                {
                    SelectedStatus = statusValues[tabIndex - 1];
                }
            }

            await GridReload();
        }

        protected string GetIcon(ActivityStatus status)
        {
            return status switch
            {
                ActivityStatus.NotStarted => "fa-play-circle",
                ActivityStatus.InProgress => "fa-clock",
                ActivityStatus.Completed => "fa-check-circle",
                ActivityStatus.Cancelled => "fa-times-circle",
                ActivityStatus.Pending => "fa-pause-circle",
                _ => "fa-question-circle"
            };
        }

        protected override string FadIcon(string iconClass)
        {
            return base.FadIcon($"fad {iconClass}");
        }
    }
}