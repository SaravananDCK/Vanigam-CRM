using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using Serilog;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingDropDownAddDataGrid<T, K> : VanigamAccountingDropDownDataGrid<T>
        where T : BaseClass where K : ComponentBase
    {        
        [Parameter] public string AddTitle { get; set; }

        [Parameter] public Dictionary<string, object> DictionaryParameter { get; set; } = null;

        [Parameter] public int Width { get; set; } = 75;

        [Parameter] public int Height { get; set; } = 100;
        
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
            ShowAdd = true;
            Add = EventCallback.Factory.Create<MouseEventArgs>(this, OnAdd);
        }
        private async Task OnAdd()
        {           
            var result = await DialogService.OpenDialogAsync<K>(AddTitle,DictionaryParameter,Width,Height);
            if (result != null)
            {
                await ValueChanged.InvokeAsync(((BaseClass)result).Oid);
                SelectedItem = result;
                EditContext.NotifyFieldChanged(EditContext.Field(nameof(FieldIdentifier.FieldName)));
            }
        }
    }
    public class VanigamAccountingDropDownDataGrid<T> : RadzenDropDownDataGrid<Guid?> where T : BaseClass
    {
        protected bool SelectedFirstItemByDefault { get; set; } = false;
        [Parameter]
        public Guid? DefaultValue { get; set; }
        [Parameter]
        public BaseApiService<T> ApiService { get; set; }

        [CascadingParameter(Name = nameof(NotificationService))]
        public NotificationService NotificationService { get; set; }

        [CascadingParameter(Name = nameof(DialogService))]
        public DialogService DialogService { get; set; }

        private bool IsInitialized { get; set; } = false;

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (!IsInitialized)
            {
                IsInitialized = true;

                ValueProperty = nameof(BaseClass.Oid);
                if (string.IsNullOrEmpty(TextProperty))
                {
                    TextProperty = nameof(NamedClass.Name);
                }

                AllowClear = true;
                //Style = "display: block; width: 100%";
                LoadData = EventCallback.Factory.Create<LoadDataArgs>(this, CustomLoadData);
                AllowFiltering = true;
                AllowFilteringByAllStringColumns = true;
                PageSize = 10;
                PagerHorizontalAlign = HorizontalAlign.Left;
                ValueChanged = EventCallback.Factory.Create<Guid?>(this, ValueHasChanged);
                await base.SetParametersAsync(parameters);
            }
        }

        protected virtual string GetCustomFilter(LoadDataArgs args)
        {
            return $"{TextProperty.GetContainsFilter(args.Filter)}";
        }
        protected virtual string GetCustomExpand()
        {
            return String.Empty;
        }

        protected async Task ValueHasChanged(Guid? args)
        {

        }
        protected async Task CustomLoadData(LoadDataArgs args)
        {
            try
            {
                var name = this.GetType().FullName;
                var customFilter = GetCustomFilter(args);
                args= GetCustomized(args);
                ODataServiceResult<T> result = await GetDataSource(args, customFilter);
                Data = result.Value.AsODataEnumerable();
                Count = result.Count;
                if (Value == null )
                {
                    if (SelectedFirstItemByDefault && Count == 1)
                    {
                        Value = ((ODataEnumerable<T>)Data).FirstOrDefault()?.Oid;
                        await ValueChanged.InvokeAsync((Guid?)Value);
                    }
                    else if (DefaultValue!=null)
                    {
                        Value = DefaultValue;
                        await ValueChanged.InvokeAsync((Guid?)Value);
                    }
                    
                    //if (EditContext != null)
                    //{
                    //    EditContext.NotifyFieldChanged(EditContext.Field(FieldIdentifier.FieldName));
                    //}
                }
                if (Value != null)
                {
                    this.SelectItemFromValue(Value);
                }
                if (SelectedItem == null)
                {
                    if (!Equals(Value, null))
                    {
                        var valueResult = await GetSelectedEntity();
                        var firstItem = valueResult.Value.FirstOrDefault();
                        if (firstItem != null)
                        {
                            SelectedItem = firstItem;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.NotificationService?.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = $"Error", Detail = $"{ex.Message}" });
                Log.Logger.Error(ex, ex?.StackTrace);
            }
        }

        protected virtual LoadDataArgs GetCustomized(LoadDataArgs args)
        {
            return args;
        }

        protected virtual async Task<ODataServiceResult<T>> GetDataSource(LoadDataArgs args, string customFilter)
        {
            return await ApiService.Get(top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null, filter: customFilter, orderBy: $"{args.OrderBy}", expand: GetCustomExpand());
        }
        protected virtual async Task<ODataServiceResult<T>> GetSelectedEntity()
        {
            return await ApiService.Get(filter: $"Oid eq {Value}", expand: GetCustomExpand());
        }
    }
}

