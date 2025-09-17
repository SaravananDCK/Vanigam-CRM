using System.Net.Http;
using System.Reflection;
using System.Timers;
using Vanigam.CRM.Client.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using Radzen;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using Vanigam.CRM.Client.Components;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Layout
{

    public partial class MainLayout 
    {
        protected static Version AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? throw new NullReferenceException();

        private bool sidebarExpanded = true;
        private int TotalUnreadCount { get; set; } = 0;
        private HubConnection chatNotificationHubConnection;

        List<MenuItemDto> MenuItems { get; set; }

        [Inject]
        protected ILocalStorageService LocalStorageService { get; set; }

        protected string FaIcon(string icon) => $"<i class=\"fa fad {icon} fa-lg\"></i>";
        protected virtual string FadIcon(string icon) => $"<i class=\"fa fad {icon} fa-lg\"></i>";
        void SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
        }

        protected void ProfileMenuClick(RadzenProfileMenuItem args)
        {
            if (args.Value == "Logout")
            {
                Security.Logout();
            }
        }


        string GetLogoPath()
        {
            return @$"images\{Security?.User?.TenantId}\Logo.svg";
        }
        async Task OnOpen()
        {
            await JSRuntime.InvokeVoidAsync("eval", "setTimeout(function(){ document.getElementById('search').focus(); }, 200)");
        }
        private RenderFragment RenderMenuItem(MenuItemDto menuItem) => builder =>
        {
            builder.OpenComponent<RadzenMenuItem>(0);
            builder.AddAttribute(1, "Text", menuItem.Text);
            builder.AddAttribute(2, "Path", menuItem.Path);
            builder.AddAttribute(3, "Icon", menuItem.Icon);
            if (menuItem.ChildItems.Any())
            {
                builder.AddAttribute(4, "ChildContent", (RenderFragment)(childBuilder =>
                {
                    foreach (var childItem in menuItem.ChildItems)
                    {
                        childBuilder.AddContent(5, RenderMenuItem(childItem));
                    }
                }));
            }
            builder.CloseComponent();
        };

        private RenderFragment RenderPanelMenuItem(MenuItemDto menuItem) => builder =>
        {
            builder.OpenComponent<RadzenPanelMenuItem>(0);
            builder.AddAttribute(1, "Text", menuItem.Text);
            builder.AddAttribute(2, "Path", menuItem.Path);
            builder.AddAttribute(3, "Icon", menuItem.Icon);
            builder.AddAttribute(4, "class", "rz-ml-5");
            if (menuItem.ChildItems.Any())
            {
                builder.AddAttribute(5, "ChildContent", (RenderFragment)(childBuilder =>
                {
                    foreach (var childItem in menuItem.ChildItems)
                    {
                        childBuilder.AddContent(6, RenderPanelMenuItem(childItem));
                    }
                }));
            }

            builder.CloseComponent();
        };
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            MenuItems = GetMenuItems();
        }
        private List<MenuItemDto> GetMenuItems()
        {
            var menuItems = new List<MenuItemDto>();

            if (Security.User?.Name == ApplicationUser.TenantsAdmin)
            {
                menuItems.Add(new MenuItemDto
                {
                    Text = Localizer["TenantSetup"],
                    Icon = "medical_services",
                    ChildItems =
                    [
                        new MenuItemDto { Text = Localizer["Tenants"], Path = "application-tenants", Icon = "task" },
                        new MenuItemDto { Text = Localizer["Users"], Path = "application-users", Icon = "summarize" },
                        new MenuItemDto { Text = Localizer["Roles"], Path = "application-roles", Icon = "summarize" }
                    ]
                });
            }
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Settings"],
                Icon = "handyman",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Hangfire"], Path = "Hangfire" },
                        new MenuItemDto
                        {
                            Text = Localizer["Docx Templates"], Path = "docx-templates", Icon = FaIcon("fa-archive")
                        },
                        new MenuItemDto
                        {
                            Text = Localizer["Docx Macro Templates"], Path = "docx-macro-templates",
                            Icon = FaIcon("fa-clone")
                        },
                        new MenuItemDto { Text = Localizer["Languages"], Path = "languages", Icon = FaIcon("fa-language") }
                ]
            });
            return menuItems;
        }
        public static readonly int DateTimeNowYear = DateTime.Now.Year;

             
        private async Task SwapTenantClick()
        {
            await DialogService.OpenDialogAsync<ChooseTenant>(Localizer["ChooseTenant"], new Dictionary<string, object>(), 30, 50);
        }

        

        

       
        

      
    }
}

