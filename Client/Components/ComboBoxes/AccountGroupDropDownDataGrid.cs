using DevExpress.Drawing.Internal.Fonts.Interop;
using DevExpress.XtraPrinting.Native.Properties;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Client.Services;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.ComboBoxes
{
    public class AccountGroupDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<AccountGroup, EditAccountGroup>
    {
        [Inject] AccountGroupApiService AccountGroupApiService { get; set; }
        public AccountGroupDropDownDataGrid()
        {
            TextProperty = nameof(AccountGroup.Name);
            Name = "cbx_AccountGroupId";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ApiService = AccountGroupApiService;
            Width = 35;
            Height = 50;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            this.Columns = (builder2) =>
            {
                builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
                builder2.AddAttribute(1, "Property", nameof(AccountGroup.Name));
                builder2.AddAttribute(2, "Title", "Name");
                builder2.CloseComponent();
            };
        }

        protected override string GetCustomFilter(LoadDataArgs args)
        {
            return $"{nameof(AccountGroup.Name).GetContainsFilter(args.Filter)}";
        }
    }
}

