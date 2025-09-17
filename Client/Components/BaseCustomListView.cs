using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components;

public class BaseCustomListView<T> : ComponentBase
{
    [Inject] protected IJSRuntime JSRuntime { get; set; }
    protected int Count { get; set; }
    protected string SearchString { get; set; } = "";
    protected bool IsLoading { get; set; } = false;
    protected bool AllowColPick { get; set; } = false;
    protected bool ShowRadioButton { get; set; } = true;
    protected IEnumerable<T> DataSource { get; set; }
    protected RadzenDataGrid<T> GridControl { get; set; }
    [Inject] protected TooltipService TooltipService { get; set; }
    public BaseCustomListView()
    {
        var rf = typeof(ComponentBase).GetField("_renderFragment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pqr = typeof(ComponentBase).GetField("_hasPendingQueuedRender", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nr = typeof(ComponentBase).GetField("_hasNeverRendered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rf.SetValue(this, (RenderFragment)(builder =>
        {
            pqr.SetValue(this, false);
            nr.SetValue(this, false);
            builder.OpenComponent<CascadingValue<TooltipService>>(1);
            builder.AddAttribute(2, "Value", TooltipService);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(2, "Name", nameof(NotificationService));
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(builder2 => BuildRenderTree(builder2)));
            builder.CloseComponent();
        }));
    }

    protected async Task Search(ChangeEventArgs args)
    {
        SearchString = $"{args.Value}";
        if (GridControl!=null)
        {
            await GridControl.GoToPage(0);
            await GridControl.Reload();
        }
    }
    protected IEnumerable<int> PageSizeOptions { get; set; } = new int[] { 10, 20, 30, 50 };
    protected int PageSize { get; set; } = 10;

    DataGridSettings _settings;
    public DataGridSettings Settings
    {
        get
        {
            return _settings;
        }
        set
        {
            if (_settings != value)
            {
                _settings = value;
                InvokeAsync(SaveStateAsync);
            }
        }
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
        }
    }
    private async Task LoadStateAsync()
    {
        var result = await JSRuntime.InvokeAsync<string>("window.localStorage.getItem", this.GetType().FullName);
        if (!string.IsNullOrEmpty(result) && result != "null")
        {
            _settings = JsonSerializer.Deserialize<DataGridSettings>(result);
            if (_settings.PageSize.HasValue)
            {
                PageSize = _settings.PageSize.Value;
                await Task.Yield();
            }
        }
    }

    private async Task SaveStateAsync()
    {
        await JSRuntime.InvokeVoidAsync("window.localStorage.setItem", this.GetType().FullName, JsonSerializer.Serialize<DataGridSettings>(Settings));
    }
    protected void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }
}
