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
    public partial class Jobs
    {
        [Parameter] public Guid? CustomerId { get; set; }
        [Parameter] public bool IsEmbeddedMode { get; set; } = false;
        [Parameter] public string? EmbeddedTitle { get; set; }

        private JobStatus? SelectedStatus = null;
        private Dictionary<JobStatus, int> StatusCounts = new();
        private int TotalCount = 0;
        private int SelectedTabIndex = 0;
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                // In embedded mode, avoid navigation property sorting that can cause server errors
                var orderBy = args.OrderBy;
                if (IsEmbeddedMode && !string.IsNullOrEmpty(orderBy) && orderBy.Contains("Customer"))
                {
                    orderBy = "Title"; // Default to sorting by Title in embedded mode
                }

                var result = await JobApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: orderBy, top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;

                // Load status counts for filter bar
                await LoadStatusCounts();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = ex.Message });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<Job>()
                .FilterByAnd(args.Filter);

            // Add customer filter if in embedded mode
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                filter = filter.FilterByAnd(j => j.PartyId == CustomerId.Value);
            }

            // Add status filter if selected
            if (SelectedStatus.HasValue)
            {
                filter = filter.FilterByAnd($"Status eq '{SelectedStatus.Value}'");
            }

            // Add search filter only if there's a search string
            filter = filter.BeginGroup()
                .ContainsOr(u => u.Title, SearchString)
                .ContainsOr(u => u.Description, SearchString)
                .EndGroup();
            return filter.Build();
        }
        protected override string GetExpandString(LoadDataArgs args)
        {
            return new ODataExpand<Job>()
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }
        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode with a customer, pre-set the CustomerId
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                parameters.Add("CustomerId", CustomerId.Value);
            }

            await DialogService.OpenDialogWithOutHeaderAsync<EditJob>(Localizer["AddJob"], parameters.Any() ? parameters : null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Job> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Job job)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditJob>(Localizer["EditJob"], new Dictionary<string, object> { { "Oid", job.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Job job)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await JobApiService.Delete(oid: job.Oid);

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

                var summary = await JobApiService.GetStatusSummaryAsync(request);
                TotalCount = summary.TotalCount;
                StatusCounts = summary.StatusCounts.ToDictionary(kv => (JobStatus)kv.Key, kv => kv.Value);
            }
            catch (Exception ex)
            {
                // Fallback to zero counts if API call fails
                StatusCounts.Clear();
                TotalCount = 0;
                foreach (JobStatus status in Enum.GetValues<JobStatus>())
                {
                    StatusCounts[status] = 0;
                }
            }
        }

        private string GetBaseFilterString()
        {
            var filter = new ODataFilter<Job>();

            // Add customer filter if in embedded mode
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                filter = filter.FilterByAnd(j => j.PartyId == CustomerId.Value);
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.Title, SearchString)
                    .ContainsOr(u => u.Description, SearchString)
                    .EndGroup();
            }

            return filter.Build();
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
                var statusValues = Enum.GetValues<JobStatus>();
                if (tabIndex - 1 < statusValues.Length)
                {
                    SelectedStatus = statusValues[tabIndex - 1];
                }
            }

            await GridReload();
        }

        protected string GetIcon(JobStatus status)
        {
            return status switch
            {
                JobStatus.Pending => "fa-clock",
                JobStatus.Assigned => "fa-user-check",
                JobStatus.Scheduled => "fa-calendar-check",
                JobStatus.InProgress => "fa-spinner",
                JobStatus.OnHold => "fa-pause-circle",
                JobStatus.Completed => "fa-check-circle",
                JobStatus.Cancelled => "fa-times-circle",
                JobStatus.Closed => "fa-archive",
                _ => "fa-question-circle"
            };
        }

        protected string GetIconColor(JobStatus status)
        {
            return status switch
            {
                JobStatus.Pending => "var(--rz-warning)",
                JobStatus.Assigned => "var(--rz-info)",
                JobStatus.Scheduled => "var(--rz-primary)",
                JobStatus.InProgress => "var(--rz-success)",
                JobStatus.OnHold => "var(--rz-secondary)",
                JobStatus.Completed => "var(--rz-success)",
                JobStatus.Cancelled => "var(--rz-danger)",
                JobStatus.Closed => "var(--rz-secondary)",
                _ => "var(--rz-text-color)"
            };
        }

        // Overloaded methods for nullable and non-nullable status types
        protected BadgeStyle GetStatusBadgeStyle(JobStatus? status)
        {
            if (!status.HasValue)
                return BadgeStyle.Info; // For "All" option

            return GetStatusBadgeStyle(status.Value);
        }

        protected BadgeStyle GetStatusBadgeStyle(JobStatus status)
        {
            return status switch
            {
                JobStatus.Pending => BadgeStyle.Warning,
                JobStatus.Assigned => BadgeStyle.Info,
                JobStatus.Scheduled => BadgeStyle.Primary,
                JobStatus.InProgress => BadgeStyle.Success,
                JobStatus.OnHold => BadgeStyle.Secondary,
                JobStatus.Completed => BadgeStyle.Success,
                JobStatus.Cancelled => BadgeStyle.Danger,
                JobStatus.Closed => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }

        protected int GetStatusCount(JobStatus status)
        {
            if (StatusCounts == null)
                return 0;

            return StatusCounts.TryGetValue(status, out var count) ? count : 0;
        }
    }
}