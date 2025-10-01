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
        [Parameter] public string EmbeddedTitle { get; set; }
        private OpportunityStage? SelectedStage = null;
        private Dictionary<OpportunityStage, int> StageCounts = new();
        private int TotalCount = 0;
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await OpportunityApiService.Get(filter: GetFilterString(args),expand:GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;

                // Load stage counts only in non-embedded mode
                if (!IsEmbeddedMode)
                {
                    await LoadStageCounts();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

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
            if (SelectedStage.HasValue)
            {
                filter = filter.FilterByAnd($"Stage eq '{SelectedStage.Value}'");
            }

            filter.BeginGroup()
                .ContainsOr(u => u.Title, SearchString)
                .EndGroup();

            return filter.Build();
        }
        protected override string GetExpandString(LoadDataArgs args)
        {
            return new ODataExpand<Opportunity>()
                .Expand(f => f.Lead, f => f.Lead.Name)
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                parameters.Add("LeadId", LeadId.Value);
            }

            await DialogService.OpenDialogAsync<EditOpportunity>(Localizer["Add Opportunity"], parameters.Count > 0 ? parameters : null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Opportunity> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Opportunity opportunity)
        {
            await DialogService.OpenDialogAsync<EditOpportunity>(Localizer["Edit Opportunity:"] + opportunity.Title, new Dictionary<string, object> { { "Oid", opportunity.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Opportunity opportunity)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await OpportunityApiService.Delete(oid:opportunity.Oid);

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

        private async Task LoadStageCounts()
        {
            try
            {
                var request = new StatusSummaryRequest
                {
                    SearchFilter = GetBaseFilterString()
                };

                var summary = await OpportunityApiService.GetStatusSummaryAsync(request);
                TotalCount = summary.TotalCount;
                StageCounts = summary.StatusCounts.ToDictionary(kv => (OpportunityStage)kv.Key, kv => kv.Value);
            }
            catch (Exception ex)
            {
                // Fallback to zero counts if API call fails
                StageCounts.Clear();
                TotalCount = 0;
                foreach (OpportunityStage stage in Enum.GetValues<OpportunityStage>())
                {
                    StageCounts[stage] = 0;
                }
            }
        }

        private string GetBaseFilterString()
        {
            var filter = new ODataFilter<Opportunity>();

            // Filter by parent Lead if in embedded mode
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.LeadId == LeadId.Value);
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.Title, SearchString)
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task OnStageChange(OpportunityStage? value)
        {
            SelectedStage = value;
            await GridReload();
        }

        // Overloaded methods for nullable and non-nullable stage types
        protected BadgeStyle GetStageBadgeStyle(OpportunityStage? stage)
        {
            if (!stage.HasValue)
                return BadgeStyle.Info; // For "All" option

            return GetStageBadgeStyle(stage.Value);
        }

        protected BadgeStyle GetStageBadgeStyle(OpportunityStage stage)
        {
            return stage switch
            {
                OpportunityStage.Prospecting => BadgeStyle.Light,
                OpportunityStage.Qualified => BadgeStyle.Info,
                OpportunityStage.Proposal => BadgeStyle.Warning,
                OpportunityStage.Negotiation => BadgeStyle.Primary,
                OpportunityStage.ClosedWon => BadgeStyle.Success,
                OpportunityStage.ClosedLost => BadgeStyle.Danger,
                _ => BadgeStyle.Secondary
            };
        }

        protected async Task OnStageTabChange(int tabIndex)
        {
            if (tabIndex == 0)
            {
                // "All" tab selected
                SelectedStage = null;
            }
            else
            {
                // Stage tab selected (tabIndex - 1 because first tab is "All")
                var stageValues = Enum.GetValues<OpportunityStage>();
                if (tabIndex - 1 < stageValues.Length)
                {
                    SelectedStage = stageValues[tabIndex - 1];
                }
            }

            await GridReload();
        }

        protected string GetIcon(OpportunityStage stage)
        {
            return stage switch
            {
                OpportunityStage.Prospecting => "fa-search",
                OpportunityStage.Qualified => "fa-filter",
                OpportunityStage.Proposal => "fa-file-alt",
                OpportunityStage.Negotiation => "fa-handshake",
                OpportunityStage.ClosedWon => "fa-trophy",
                OpportunityStage.ClosedLost => "fa-times-circle",
                _ => "fa-question-circle"
            };
        }

        protected string GetIconColor(OpportunityStage stage)
        {
            return stage switch
            {
                OpportunityStage.Prospecting => "var(--rz-info)",
                OpportunityStage.Qualified => "var(--rz-primary)",
                OpportunityStage.Proposal => "var(--rz-secondary)",
                OpportunityStage.Negotiation => "var(--rz-warning)",
                OpportunityStage.ClosedWon => "var(--rz-success)",
                OpportunityStage.ClosedLost => "var(--rz-danger)",
                _ => "var(--rz-text-color)"
            };
        }

        protected int GetStageCount(OpportunityStage stage)
        {
            if (StageCounts == null)
                return 0;

            return StageCounts.TryGetValue(stage, out var count) ? count : 0;
        }
    }
}