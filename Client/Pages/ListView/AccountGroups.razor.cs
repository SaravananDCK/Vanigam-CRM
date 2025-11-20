using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class AccountGroups
    {
        private IEnumerable<AccountGroup> TreeDataSource = new List<AccountGroup>();
        private object? SelectedItem;
        private int TotalCount = 0;
        private IEnumerable<AccountGroup> AllAccountGroups = new List<AccountGroup>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadTreeData();
        }

        private async Task LoadTreeData()
        {
            try
            {
                // Load all account groups with parent relationships
                var result = await AccountGroupApiService.Get(
                    filter: GetSearchFilter(),
                    expand: "ParentGroup,ChildGroups",
                    orderBy: "Name",
                    top: 5000);

                AllAccountGroups = result.Value.AsODataEnumerable();
                TotalCount = AllAccountGroups.Count();

                // Build tree structure - only root nodes (no parent)
                TreeDataSource = AllAccountGroups
                    .Where(ag => ag.ParentGroupId == null)
                    .Select(BuildTree)
                    .ToList();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer[$"Error"],
                    Detail = ex.Message
                });
            }
        }

        private AccountGroup BuildTree(AccountGroup node)
        {
            // Find children from the flat list
            var children = AllAccountGroups
                .Where(ag => ag.ParentGroupId == node.Oid)
                .Select(BuildTree)
                .ToList();

            if (children.Any())
            {
                node.ChildGroups = children;
            }

            return node;
        }

        private bool ShouldExpand(AccountGroup accountGroup)
        {
            // Expand first level by default
            return accountGroup.ParentGroupId == null;
        }

        private string GetSearchFilter()
        {
            if (string.IsNullOrEmpty(SearchString))
                return string.Empty;

            return new ODataFilter<AccountGroup>()
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Code, SearchString)
                .EndGroup()
                .Build();
        }

        protected virtual async Task Search(ChangeEventArgs args)
        {
            await LoadTreeData();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditAccountGroup>(Localizer["AddAccountGroup"], null, 100, 100);
            await LoadTreeData();
        }

        private async Task OpenEditDialog(AccountGroup accountGroup)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditAccountGroup>(
                Localizer["EditAccountGroup"],
                new Dictionary<string, object> { { "Oid", accountGroup.Oid } },
                100, 100);
            await LoadTreeData();
        }

        private async Task DeleteAccountGroup(AccountGroup accountGroup)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await AccountGroupApiService.Delete(oid: accountGroup.Oid);

                    if (deleteResult != null)
                    {
                        await LoadTreeData();
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
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer[$"Error"],
                    Detail = Localizer[$"UnableDelete"]
                });
            }
        }

        // Keep these for base class compatibility
        protected async Task GridLoadData(LoadDataArgs args)
        {
            await LoadTreeData();
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return GetSearchFilter();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
            return "ParentGroup,ChildGroups";
        }
    }
}
