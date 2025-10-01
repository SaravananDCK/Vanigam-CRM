using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Client.Services;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Quotes
    {
        [Parameter] public Guid? JobId { get; set; }
        [Parameter] public bool IsEmbeddedMode { get; set; } = false;
        [Parameter] public string? EmbeddedTitle { get; set; }
        [Inject] private PdfApiService PdfApiService { get; set; }

        private QuoteStatus? SelectedStatus = null;
        private Dictionary<QuoteStatus, int> StatusCounts = new();
        private int TotalCount = 0;
        private int SelectedTabIndex = 0;

        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await QuoteApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;

                // Load status counts for filter bar
                await LoadStatusCounts();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<Quote>()
                .FilterByAnd(args.Filter);

            // Filter by parent Job if in embedded mode
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.JobId == JobId.Value);
            }

            // Add status filter if selected
            if (SelectedStatus.HasValue)
            {
                filter = filter.FilterByAnd($"Status eq '{SelectedStatus.Value}'");
            }

            return filter
                .BeginGroup()
                .ContainsOr(u => u.Number, SearchString)
                .EndGroup()
                .Build();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
            return new ODataExpand<Quote>()
                .Expand(f => f.Opportunity, f => f.Opportunity.Title)
                .Expand(f => f.Party, f => f.Party.Name)
                .Expand(f => f.Job, f => f.Job.Title)
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

                var summary = await QuoteApiService.GetStatusSummaryAsync(request);
                TotalCount = summary.TotalCount;
                StatusCounts = summary.StatusCounts.ToDictionary(kv => (QuoteStatus)kv.Key, kv => kv.Value);
            }
            catch (Exception ex)
            {
                // Fallback to zero counts if API call fails
                StatusCounts.Clear();
                TotalCount = 0;
                foreach (QuoteStatus status in Enum.GetValues<QuoteStatus>())
                {
                    StatusCounts[status] = 0;
                }
            }
        }

        private string GetBaseFilterString()
        {
            var filter = new ODataFilter<Quote>();

            // Filter by parent Job if in embedded mode
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.JobId == JobId.Value);
            }

            filter = filter.BeginGroup()
                .ContainsOr(u => u.Number, SearchString)
                .EndGroup();
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
                var statusValues = Enum.GetValues<QuoteStatus>();
                if (tabIndex - 1 < statusValues.Length)
                {
                    SelectedStatus = statusValues[tabIndex - 1];
                }
            }

            await GridReload();
        }

        protected string GetIcon(QuoteStatus status)
        {
            return status switch
            {
                QuoteStatus.Draft => "fa-edit",
                QuoteStatus.Sent => "fa-paper-plane",
                QuoteStatus.Accepted => "fa-check-circle",
                QuoteStatus.Rejected => "fa-times-circle",
                QuoteStatus.Expired => "fa-clock",
                _ => "fa-question-circle"
            };
        }

        protected string GetIconColor(QuoteStatus status)
        {
            return status switch
            {
                QuoteStatus.Draft => "var(--rz-secondary)",
                QuoteStatus.Sent => "var(--rz-info)",
                QuoteStatus.Accepted => "var(--rz-success)",
                QuoteStatus.Rejected => "var(--rz-danger)",
                QuoteStatus.Expired => "var(--rz-warning)",
                _ => "var(--rz-text-color)"
            };
        }
        // Overloaded methods for nullable and non-nullable status types
        protected BadgeStyle GetStatusBadgeStyle(QuoteStatus? status)
        {
            if (!status.HasValue)
                return BadgeStyle.Info; // For "All" option

            return GetStatusBadgeStyle(status.Value);
        }

        protected BadgeStyle GetStatusBadgeStyle(QuoteStatus status)
        {
            return status switch
            {
                QuoteStatus.Draft => BadgeStyle.Secondary,
                QuoteStatus.Sent => BadgeStyle.Info,
                QuoteStatus.Accepted => BadgeStyle.Success,
                QuoteStatus.Rejected => BadgeStyle.Danger,
                QuoteStatus.Expired => BadgeStyle.Warning,
                _ => BadgeStyle.Light
            };
        }

        protected int GetStatusCount(QuoteStatus status)
        {
            if (StatusCounts == null)
                return 0;

            return StatusCounts.TryGetValue(status, out var count) ? count : 0;
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && JobId.HasValue)
            {
                parameters.Add("JobId", JobId.Value);
            }

            await DialogService.OpenDialogWithOutHeaderAsync<EditQuote>(Localizer["AddQuote"], parameters.Count > 0 ? parameters : null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Quote> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Quote quote)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditQuote>(Localizer["EditQuote"], new Dictionary<string, object> { { "Oid", quote.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Quote quote)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await QuoteApiService.Delete(oid: quote.Oid);

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

        protected async Task PreviewPdf(Quote quote)
        {
            try
            {
                await PdfApiService.PreviewQuotePdfAsync(quote.Oid);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["ErrorPreviewingPdf"]
                });
            }
        }

        protected async Task DownloadPdf(Quote quote)
        {
            try
            {
                await PdfApiService.DownloadQuotePdfAsync(quote.Oid);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["ErrorDownloadingPdf"]
                });
            }
        }
    }
}