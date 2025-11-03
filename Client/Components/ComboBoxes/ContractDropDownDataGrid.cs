using DevExpress.Drawing.Internal.Fonts.Interop;
using DevExpress.XtraPrinting.Native.Properties;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Radzen;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Objects.Entities;
using Radzen.Blazor;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.ComboBoxes
{
    public class ContractDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<Contract, EditContract>
    {
        [Inject] ContractApiService ContractApiService { get; set; }
        public ContractDropDownDataGrid()
        {
            TextProperty = nameof(Contract.Title);
            Name = "cbx_ContractId";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ApiService = ContractApiService;
            Width = 35;
            Height = 50;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            this.Columns = (builder2) =>
            {
                builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
                builder2.AddAttribute(1, "Property", nameof(Contract.Title));
                builder2.AddAttribute(2, "Title", "Title");
                builder2.CloseComponent();
            };
        }

        protected override string GetCustomFilter(LoadDataArgs args)
        {
            return $"{nameof(Contract.Title).GetContainsFilter(args.Filter)}";
        }
    }
}

