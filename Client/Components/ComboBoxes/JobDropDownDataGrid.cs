using DevExpress.Drawing.Internal.Fonts.Interop;
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
    public class JobDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<Job, EditJob>
    {
        [Inject] JobApiService JobApiService { get; set; }
        public JobDropDownDataGrid()
        {
            TextProperty = nameof(Job.Title);
            Name = "cbx_JobId";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ApiService = JobApiService;
            Width = 35;
            Height = 50;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            this.Columns = (builder2) =>
            {
                builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
                builder2.AddAttribute(1, "Property", nameof(Job.Number));
                builder2.AddAttribute(2, "Title", "Number");
                builder2.CloseComponent();

                builder2.OpenComponent<RadzenDropDownDataGridColumn>(1);
                builder2.AddAttribute(1, "Property", nameof(Job.Title));
                builder2.AddAttribute(2, "Title", "Title");
                builder2.CloseComponent();
            };
        }

        protected override string GetCustomFilter(LoadDataArgs args)
        {
            return $"{nameof(Job.Number).GetContainsFilter(args.Filter)} or {nameof(Job.Title).GetContainsFilter(args.Filter)}";
        }
    }
}
