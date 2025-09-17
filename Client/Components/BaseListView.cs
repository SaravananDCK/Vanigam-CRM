using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Client.Components
{
    public abstract class BaseListView<T, K> : BaseView<T, K> where T : BaseClass where K : ComponentBase
    {

        protected int Count { get; set; }
        protected string SearchString { get; set; } = "";
        protected bool IsLoading { get; set; } = false;
        protected bool AllowColPick { get; set; } = false;
        protected bool ShowRadioButton { get; set; } = true;
        protected IEnumerable<T> DataSource { get; set; }
        protected IEnumerable<T> CardDataSource { get; set; }
        protected RadzenDataGrid<T> GridControl { get; set; }
        protected RadzenDataList<T> DataListControl { get; set; }

        [Parameter] public ViewTypes ViewType { get; set; }

        [CascadingParameter(Name = nameof(GridDynamicHeightValue))]
        protected int GridDynamicHeightValue { get; set; } = 280;

        [CascadingParameter(Name = nameof(RegionId))]
        public Guid? RegionId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            DataSource = Enumerable.Empty<T>().AsODataEnumerable();
        }
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            bool regionChanged = false;
            foreach (var param in parameters)
            {
                if (param.Name == nameof(RegionId) && (Guid?)param.Value != RegionId)
                {
                    regionChanged = true;
                    break;
                }
            }
            await base.SetParametersAsync(parameters);
            if (regionChanged)
            {
                await OnRegionChanged();
            }

        }

        protected virtual async Task OnRegionChanged()
        {
            if (GridControl != null)
            {
                await GridReload();
            }
        }
        protected async Task Search(ChangeEventArgs args)
        {
            SearchString = $"{args.Value}";
            await GridControl.GoToPage(0);
            await GridReload();
        }

        protected IEnumerable<int> PageSizeOptions { get; set; } = new int[] { 10, 20, 30, 50 };
        protected int PageSize { get; set; } = 10;

        DataGridSettings _settings;

        public DataGridSettings Settings
        {
            get { return _settings; }
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

        protected virtual string GetFilterString(LoadDataArgs args)
        {
            return string.Empty;
        }

        protected virtual string GetExpandString(LoadDataArgs args)
        {
            return string.Empty;
        }

        private bool isStateLoaded = false;
        protected async Task LoadStateAsync()
        {
            if (!isStateLoaded)
            {
                isStateLoaded = true;
                var result = await LocalStorageService.GetItemAsync(this.GetType().FullName);
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
        }

        protected async Task GridReload()
        {
            if (GridControl != null)
            {
                await GridControl.Reload()!;
            }
        }
        protected async Task LoadColumnFilterData(DataGridLoadColumnFilterDataEventArgs<T> args)
        {
            // Get the property name in OData format. Sub-properties are separated by /.
            var property = args.Column.GetFilterProperty().Replace(".", "/");

            // Get the distinct values for the property in OData format for the current column.
            var result = await GetDistinctColumnValues(
                count: true,
                filter: !string.IsNullOrEmpty(args.Filter) ? $"contains(tolower({property}), tolower('{args.Filter}'))" : null,
                orderBy: $"{property}",
                apply: $"groupby(({property}))",
                expand: GetODataExpand(property));
            args.Count = result.Count;
            args.Data = result.Value;
        }

        protected virtual Task<ODataServiceResult<T>> GetDistinctColumnValues(string filter = default(string), string expand = default(string), string orderBy = default(string), string apply = default(string), bool? count = default(bool?))
        {
            return Task.FromResult(new ODataServiceResult<T>());
        }

        protected string GetODataExpand(string property)
        {
            var properties = property.Split("/");
            return properties.Count() > 1 ? $"{string.Join("/", properties.Take(properties.Length - 1))}($select={properties.LastOrDefault()})" : null;
        }
        private async Task SaveStateAsync()
        {
            await LocalStorageService.SetItemAsync(this.GetType().FullName, JsonSerializer.Serialize<DataGridSettings>(Settings));
        }
        protected void LoadSettings(DataGridLoadSettingsEventArgs args)
        {
            if (Settings != null)
            {
                args.Settings = Settings;
            }
        }
    }
    public enum ViewTypes
    {
        ListView,
        PopUpView
    }
}

