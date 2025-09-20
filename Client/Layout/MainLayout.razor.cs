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
using Vanigam.CRM.Client.Services;

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

        protected virtual string FaIcon(string icon) => FontAwesomeIconMapping.GetString(icon);
        protected virtual string FadIcon(string icon) => FontAwesomeIconMapping.GetString(icon);
        protected string FasIcon(string icon) => FontAwesomeIconMapping.GetString(icon);
        protected string FarIcon(string icon) => FontAwesomeIconMapping.GetString(icon);
        protected string FabIcon(string icon) => FontAwesomeIconMapping.GetString(icon);
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
            builder.AddAttribute(3, "Icon", FontAwesomeIconMapping.GetUnicode(menuItem.Icon));
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

            // CRM Core - Customer Relationship Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["CRM"],
                Icon = "fa-people",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Leads"], Path = "leads", Icon = ("fa-user-plus") },
                    new MenuItemDto { Text = Localizer["Opportunities"], Path = "opportunities", Icon = ("fa-handshake") },
                    new MenuItemDto { Text = Localizer["Activities"], Path = "activities", Icon = ("fa-tasks") },
                    new MenuItemDto { Text = Localizer["Customers"], Path = "customers", Icon = "fa-users" },
                    new MenuItemDto { Text = Localizer["Contacts"], Path = "contacts", Icon = "fa-address-book" },
                ]
            });

            // Job Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Jobs"],
                Icon = "fa-briefcase",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Jobs"], Path = "jobs", Icon = ("fa-briefcase") },
                    new MenuItemDto { Text = Localizer["Job Assignments"], Path = "jobassignments", Icon = ("fa-user-tie") },
                    new MenuItemDto { Text = Localizer["Job Reports"], Path = "jobreports", Icon = ("fa-file-alt") },
                    new MenuItemDto { Text = Localizer["Appointments"], Path = "appointments", Icon = ("fa-calendar") },
                    new MenuItemDto { Text = Localizer["Recurring Jobs"], Path = "recurringjobs", Icon = ("fa-sync") }
                ]
            });

            // Financial Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Finance"],
                Icon = "fa-coins",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Quotes"], Path = "quotes", Icon = ("fa-file-invoice-dollar") },
                    new MenuItemDto { Text = Localizer["Quote Items"], Path = "quoteitems", Icon = ("fa-list-ul") },
                    new MenuItemDto { Text = Localizer["Invoices"], Path = "invoices", Icon = ("fa-file-invoice") },
                    new MenuItemDto { Text = Localizer["Payments"], Path = "payments", Icon = ("fa-credit-card") },
                    new MenuItemDto { Text = Localizer["Contracts"], Path = "contracts", Icon = ("fa-handshake") }
                ]
            });

            // Operations Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Operations"],
                Icon = "fa-hammer",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Technicians"], Path = "technicians", Icon = ("fa-hard-hat") },
                    new MenuItemDto { Text = Localizer["Time Sheets"], Path = "timesheets", Icon = ("fa-clock") },
                    new MenuItemDto { Text = Localizer["Vehicles"], Path = "vehicles", Icon = ("fa-truck") },
                    new MenuItemDto { Text = Localizer["GPS Points"], Path = "gpspoints", Icon = ("fa-map-marker") },
                    new MenuItemDto { Text = Localizer["Locations"], Path = "locations", Icon = ("fa-map-pin") }
                ]
            });

            // Inventory Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Inventory"],
                Icon = "fa-inventory",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Inventory Items"], Path = "inventoryitems", Icon = ("fa-boxes") },
                    new MenuItemDto { Text = Localizer["Material Usage"], Path = "materialusages", Icon = ("fa-wrench") }
                ]
            });

            // System Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["System"],
                Icon = "fa-gear",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Notifications"], Path = "notifications", Icon = ("fa-bell") },
                    new MenuItemDto { Text = Localizer["User Sessions"], Path = "usersessions", Icon = ("fa-user-clock") },
                    new MenuItemDto { Text = Localizer["Audit Logs"], Path = "auditlogs", Icon = ("fa-history") },
                    new MenuItemDto { Text = Localizer["Custom Fields"], Path = "customfields", Icon = ("fa-cogs") },
                    new MenuItemDto { Text = Localizer["Feedback"], Path = "feedbacks", Icon = ("fa-comments") }
                ]
            });

            // Document Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Documents"],
                Icon = "fa-file",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["File Documents"], Path = "filedocuments", Icon = ("fa-file") },
                    new MenuItemDto { Text = Localizer["File Categories"], Path = "filecategories", Icon = ("fa-folder") },
                    new MenuItemDto { Text = Localizer["Attachments"], Path = "attachments", Icon = ("fa-paperclip") },
                    new MenuItemDto { Text = Localizer["Report Templates"], Path = "reporttemplates", Icon = ("fa-file-code") }
                ]
            });

            // Service Level Management
            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Service Level"],
                Icon = "fa-headset",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["SLAs"], Path = "slas", Icon = ("fa-stopwatch") }
                ]
            });

            if (Security.User?.Name == ApplicationUser.TenantsAdmin)
            {
                menuItems.Add(new MenuItemDto
                {
                    Text = Localizer["TenantSetup"],
                    Icon = "fa-toolbox",
                    ChildItems =
                    [
                        new MenuItemDto { Text = Localizer["Tenants"], Path = "application-tenants", Icon = "business" },
                        new MenuItemDto { Text = Localizer["Users"], Path = "application-users", Icon = "person" },
                        new MenuItemDto { Text = Localizer["Roles"], Path = "application-roles", Icon = "security" }
                    ]
                });
            }

            menuItems.Add(new MenuItemDto
            {
                Text = Localizer["Settings"],
                Icon = "fa-toolbox",
                ChildItems =
                [
                    new MenuItemDto { Text = Localizer["Hangfire"], Path = "Hangfire", Icon = ("fa-cog") },
                    new MenuItemDto { Text = Localizer["Docx Templates"], Path = "docx-templates", Icon = ("fa-archive") },
                    new MenuItemDto { Text = Localizer["Docx Macro Templates"], Path = "docx-macro-templates", Icon = ("fa-clone") },
                    new MenuItemDto { Text = Localizer["Languages"], Path = "languages", Icon = ("fa-language") }
                ]
            });

            return menuItems;
        }
        public static readonly int DateTimeNowYear = DateTime.Now.Year;

             
        private async Task SwapTenantClick()
        {
            await DialogService.OpenDialogAsync<ChooseTenant>(Localizer["ChooseTenant"], new Dictionary<string, object>(), 80, 80);
        }

        

        

       
        

      
    }
}

