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
    public class BankAccountDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<BankAccount, EditBankAccount>
    {
        [Inject] BankAccountApiService BankAccountApiService { get; set; }
        public BankAccountDropDownDataGrid()
        {
            TextProperty = nameof(BankAccount.Name);
            Name = "cbx_BankAccountId";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ApiService = BankAccountApiService;
            Width = 35;
            Height = 50;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            this.Columns = (builder2) =>
            {
                builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
                builder2.AddAttribute(1, "Property", nameof(BankAccount.Name));
                builder2.AddAttribute(2, "Title", "Bank Name");
                builder2.CloseComponent();

                builder2.OpenComponent<RadzenDropDownDataGridColumn>(1);
                builder2.AddAttribute(1, "Property", nameof(BankAccount.AccountNumber));
                builder2.AddAttribute(2, "Title", "Account Number");
                builder2.CloseComponent();
            };
        }

        protected override string GetCustomFilter(LoadDataArgs args)
        {
            return $"{nameof(Contract.Title).GetContainsFilter(args.Filter)}";
        }
    }
}

