using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Components
{
    public partial class DetailPageTitleComponent
    {
        [Parameter]
        public string TitleText { get; set; }

        [Parameter]
        public Guid CurrentOid { get; set; }

        [Parameter]
        public bool HasChanges { get; set; }

        [Parameter]
        public bool CanEdit { get; set; }

        [Parameter]
        public DialogService DialogService { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> ReloadButtonClick { get; set; }

        [Parameter]
        public string Icon { get; set; }

        [Parameter]
        public string AddButtonText { get; set; }

        [Parameter] public ButtonStyle AddButtonStyle { get; set; } = ButtonStyle.Primary;

        [Parameter]
        public bool ShowSearch { get; set; } = true;

        [Parameter]
        public bool ShowAdd { get; set; } = true;

        [Parameter]
        public bool IsBusy { get; set; }

        [Parameter]
        public EventCallback<bool> IsBusyChanged { get; set; }


        [Parameter]
        public EventCallback<MouseEventArgs> AddButtonClick { get; set; }

        [Parameter]
        public EventCallback<ChangeEventArgs> SearchButtonClick { get; set; }

        [Parameter] public RenderFragment CustomButtons { get; set; }
        [Parameter] public RenderFragment CustomTitles { get; set; }
        [Parameter] public RenderFragment RadioButtons { get; set; }
        [Parameter] public RenderFragment DropDowns { get; set; }
        [Parameter] public RenderFragment CustomBadge { get; set; }

        [Parameter] public int FirstSizeMD { get; set; } = 4;
        [Parameter] public int MiddleSizeMD { get; set; } = 2;
        [Parameter] public int LastSizeMD { get; set; } = 6;
        [Parameter] public int Size { get; set; } = 12;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                if (string.IsNullOrEmpty(AddButtonText))
                {
                    AddButtonText = Localizer["Add"];
                }
            }
        }

        private async Task OnCloseClick(MouseEventArgs arg)
        {
            DialogService.CloseDialog();
        }
    }
}
