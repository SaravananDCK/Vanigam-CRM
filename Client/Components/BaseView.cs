using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Radzen;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Client.Components;

public class CoreView: ComponentBase
{
    [Inject] protected IJSRuntime JSRuntime { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; }

    [Inject] protected DialogService DialogService { get; set; }

    [Inject] protected TooltipService TooltipService { get; set; }

    [Inject] protected ContextMenuService ContextMenuService { get; set; }

    protected virtual string FaIcon(string icon) => $"<i class=\"fa fa-solid {icon}\"></i>";
    protected virtual string FadIcon(string icon) => $"<i class=\"fa fad {icon} fa-lg\"></i>";
    protected virtual string FadIconSmall(string icon) => $"<i class=\"fa fad {icon}\"></i>";
    [Inject] protected NotificationService NotificationService { get; set; }
    [Inject] protected SecurityService Security { get; set; }
    [Inject] protected ILocalStorageService LocalStorageService { get; set; }
    public ApplicationAuthenticationState AuthenticationState { get; set; }
}
public class BaseView<T, K> : CoreView where T : BaseClass where K : ComponentBase
{
    
    [Inject] protected Microsoft.Extensions.Localization.IStringLocalizer<K> Localizer { get; set; }
    [Inject] protected ILogger<K> Logger { get; set; }
    

    [Parameter]
    public bool IsBusy { get; set; }
    
    [Parameter]
    public EventCallback<bool> IsBusyChanged { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    public BaseView()
    {
        var rf = typeof(ComponentBase).GetField("_renderFragment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pqr = typeof(ComponentBase).GetField("_hasPendingQueuedRender", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nr = typeof(ComponentBase).GetField("_hasNeverRendered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rf.SetValue(this, (RenderFragment)(builder =>
        {
            pqr.SetValue(this, false);
            nr.SetValue(this, false);
            builder.OpenComponent<CascadingValue<NotificationService>>(1);
            builder.AddAttribute(2, "Value", NotificationService);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(2, "Name", nameof(NotificationService));
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(builder2 =>
            {
                builder2.OpenComponent<CascadingValue<DialogService>>(1);
                builder2.AddAttribute(2, "Value", DialogService);
                builder2.AddAttribute(2, "IsFixed", true);
                builder2.AddAttribute(2, "Name", nameof(DialogService));
                builder2.AddAttribute(3, "ChildContent", (RenderFragment)(builder3 =>
                    
                    {
                        builder3.OpenComponent<CascadingValue<TooltipService>>(1);
                        builder3.AddAttribute(2, "Value", TooltipService);
                        builder3.AddAttribute(2, "IsFixed", true);
                        builder3.AddAttribute(2, "Name", nameof(TooltipService));
                        builder3.AddAttribute(3, "ChildContent", (RenderFragment)(builder4 => BuildRenderTree(builder4)));
                        builder3.CloseComponent();
                    }));
                builder2.CloseComponent();
            }));
            builder.CloseComponent();
        }));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        AuthenticationState = await Security.GetAuthenticationStateAsync();
        var authorizeAttribute = GetType().GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault();
        if (authorizeAttribute!=null && AuthenticationState != null && !AuthenticationState.IsAuthenticated)
        {
            NavigationManager.NavigateTo(@"\login", true);
        }
    }
}
