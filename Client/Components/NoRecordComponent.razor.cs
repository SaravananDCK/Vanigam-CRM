using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Vanigam.CRM.Client.Components
{
    public partial class NoRecordComponent
    {
        [Parameter]
        public string EmptyText { get; set; } = string.Empty;
        [Parameter]
        public string ButtonText { get; set; }
        [Parameter]
        public EventCallback<MouseEventArgs> ButtonClick { get; set; }
        [Parameter]
        public bool ShowAddButton { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            if(EmptyText == string.Empty)
            {
                EmptyText = Localizer["NoRecordFound"];
            }
            await base.OnInitializedAsync();
        }
    }
}
