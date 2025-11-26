using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NodaTime;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditContract
    {
        [Inject] private ContractApiService ContractApiService { get; set; }
        
        private ContractDuration _previousDuration;
        private DateTimeOffset? _previousStartDate;
        private static readonly IList<ContractDuration> ContractDurations = [.. Enum.GetValues<ContractDuration>()];
        private static readonly IList<ContractCoverageType> ContractCoverageTypes = [.. Enum.GetValues<ContractCoverageType>()];
        
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new AmcContract();
                CurrentObject.StartDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
            }
            else
                CurrentObject = await ContractApiService.GetByOid(oid: Oid, expand: "Customer");

            // Store initial values
            _previousDuration = CurrentObject.Duration;
            _previousStartDate = CurrentObject.StartDate;
            GenerateContractTitle();
            await InitEditContext();
        }
        private async Task OnContractCoverageTypeChanged(ContractCoverageType value)
        {
            StateHasChanged();
        }
        /// <summary>
        /// Generates contract title based on duration and start date
        /// Format: "AMC {Duration} {Year}" or "AMC {Duration} {Year-Month}" for monthly contracts
        /// </summary>
        private void GenerateContractTitle()
        {
            if (CurrentObject.StartDate.HasValue)
            {
                var durationName = CurrentObject.Duration.ToString();
                var year = CurrentObject.StartDate.Value.Year;

                // For monthly contracts, include the month
                if (CurrentObject.Duration == ContractDuration.Monthly)
                {
                    var month = CurrentObject.StartDate.Value.ToString("MM");
                    CurrentObject.Title = $"AMC {durationName} {year}-{month}";
                }
                else
                {
                    CurrentObject.Title = $"AMC {durationName} {year}";
                }
            }
            else
            {
                // If no start date, just use duration
                CurrentObject.Title = $"AMC {CurrentObject.Duration}";
            }
        }

        /// <summary>
        /// Handles changes to Duration field
        /// </summary>
        private void OnDurationChanged(ContractDuration newDuration)
        {
            if (_previousDuration != newDuration)
            {
                CurrentObject.Duration = newDuration;
                _previousDuration = newDuration;
                GenerateContractTitle();
                StateHasChanged();
            }
        }

        /// <summary>
        /// Handles changes to StartDate field
        /// </summary>
        private void OnStartDateChanged(DateTimeOffset? newStartDate)
        {
            if (_previousStartDate != newStartDate)
            {
                CurrentObject.StartDate = newStartDate;
                _previousStartDate = newStartDate;
                GenerateContractTitle();
                StateHasChanged();
            }
        }
        
        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await ContractApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await ContractApiService.Update(oid: Oid, CurrentObject);
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

        protected override async Task SaveAndStayInEdit()
        {
            await FormSubmit();
            // After successful save, switch back to read-only mode
            if (!ErrorVisible && !ShowNotUniqueAlert)
            {
                IsReadOnlyMode = true;
                StateHasChanged();
            }
        }
    }
}
