using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Leads
    {
        private LeadStatus? SelectedStatus = null;
        private Dictionary<LeadStatus, int> StatusCounts = new();
        private int TotalCount = 0;
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await LeadApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;

                // Load status counts for the filter cards
                await LoadStatusCounts();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<Lead>()
                .FilterByAnd(args.Filter);

            // Add status filter if selected
            if (SelectedStatus.HasValue)
            {
                filter = filter.FilterByAnd($"Status eq '{SelectedStatus.Value}'");
            }

            return filter
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Organization, SearchString)
                .ContainsOr(u => u.Email, SearchString)
                .ContainsOr(u => u.Phone, SearchString)
                .ContainsOr(u => u.JobTitle, SearchString)
                .ContainsOr(u => u.Industry, SearchString)
                .ContainsOr(u => u.Source, SearchString)
                .ContainsOr(u => u.CampaignSource, SearchString)
                .ContainsOr(u => u.City, SearchString)
                .ContainsOr(u => u.State, SearchString)
                .ContainsOr(u => u.ProductOfInterest, SearchString)
                .ContainsOr(u => u.Comments, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditLead>(Localizer["AddLead"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Lead> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Lead lead)
        {
            await DialogService.OpenDialogAsync<EditLead>(Localizer["EditLead"], new Dictionary<string, object> { { "Oid", lead.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Lead lead)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await LeadApiService.Delete(oid:lead.Oid);

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

                var summary = await LeadApiService.GetStatusSummaryAsync(request);
                TotalCount = summary.TotalCount;
                StatusCounts = summary.StatusCounts.ToDictionary(kv => (LeadStatus)kv.Key, kv => kv.Value);
            }
            catch (Exception ex)
            {
                // Fallback to zero counts if API call fails
                StatusCounts.Clear();
                StatusCounts= null;
                foreach (LeadStatus status in Enum.GetValues<LeadStatus>())
                {
                    StatusCounts[status] = 0;
                }
            }
        }

        private string GetBaseFilterString()
        {
            if (string.IsNullOrEmpty(SearchString))
                return string.Empty;

            return new ODataFilter<Lead>()
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Organization, SearchString)
                .ContainsOr(u => u.Email, SearchString)
                .ContainsOr(u => u.Phone, SearchString)
                .ContainsOr(u => u.JobTitle, SearchString)
                .ContainsOr(u => u.Industry, SearchString)
                .ContainsOr(u => u.Source, SearchString)
                .ContainsOr(u => u.CampaignSource, SearchString)
                .ContainsOr(u => u.City, SearchString)
                .ContainsOr(u => u.State, SearchString)
                .ContainsOr(u => u.ProductOfInterest, SearchString)
                .ContainsOr(u => u.Comments, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task OnStatusChange(LeadStatus? value)
        {
            SelectedStatus = value;
            await GridReload();
        }

        protected BadgeStyle GetStatusBadgeStyle(LeadStatus? status)
        {
            if (!status.HasValue)
                return BadgeStyle.Info; // For "All" option

            return status.Value switch
            {
                LeadStatus.New => BadgeStyle.Secondary,
                LeadStatus.Contacted => BadgeStyle.Info,
                LeadStatus.Qualified => BadgeStyle.Success,
                LeadStatus.Converted => BadgeStyle.Success,
                LeadStatus.Lost => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        protected BadgeStyle GetStatusBadgeStyle(LeadStatus status)
        {
            return status switch
            {
                LeadStatus.New => BadgeStyle.Secondary,
                LeadStatus.Contacted => BadgeStyle.Info,
                LeadStatus.Qualified => BadgeStyle.Success,
                LeadStatus.Converted => BadgeStyle.Success,
                LeadStatus.Lost => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        protected int GetStatusCount(LeadStatus status)
        {
            if (StatusCounts == null)
                return 0;

            return StatusCounts.TryGetValue(status, out var count) ? count : 0;
        }
    }
}