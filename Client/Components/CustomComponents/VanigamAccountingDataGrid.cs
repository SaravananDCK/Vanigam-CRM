using System.Linq.Expressions;
using DevExpress.Data.Design;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Serilog;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingDataGrid<TItem> : RadzenDataGrid<TItem>
    {
        [Parameter]
        public int LoadCounter { get; set; }
        [Parameter]
        public EventCallback<int> LoadCounterChanged { get; set; }
        private bool IsParamtersLoaded = false;
        private IList<TItem> CurrentObject { get; set; }
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (!IsParamtersLoaded)
            {
                IsParamtersLoaded = true;
                AllowFiltering = true;
                FilterMode = FilterMode.CheckBoxList;
                AllowPaging = true;
                AllowSorting = true;
                ShowPagingSummary = true;
                AllowRowSelectOnRowClick = true;
                AllowMultiColumnSorting = false;
                ShowMultiColumnSortingIndex = true;
                AllowColumnResize = true;
                AllowColumnReorder = true;
                SelectionMode = DataGridSelectionMode.Single;
                ColumnWidth = "200px";
                PageSizeOptions = new int[] { 10, 20, 30, 50 };
                PagerAlwaysVisible=true;
                PageNumbersCount = 10;
                AllowColumnResize = true;
                PageSize = 10;
                Density= Density.Compact;
                Value = CurrentObject;
                Style = "height:100%;";
                FilterCaseSensitivity = FilterCaseSensitivity.CaseInsensitive;
                ValueChanged = EventCallback.Factory.Create<IList<TItem>>(this,
                    (IList<TItem> args) => { CurrentObject = args; });
                LoadData = EventCallback.Factory.Create<LoadDataArgs>(this, OnLoadData);
            }

            await base.SetParametersAsync(parameters);
        }
        [Parameter]
        public EventCallback<LoadDataArgs> VanigamAccountingLoadData { get; set; }
        private async Task OnLoadData(LoadDataArgs args)
        {
            LoadCounter++;
            await VanigamAccountingLoadData.InvokeAsync(args);
        }
    }
    //public class MeditalkDropDownDataGrid<T> : RadzenDropDownDataGrid<T>
    //{
    //    public override async Task SetParametersAsync(ParameterView parameters)
    //    {
    //        await base.SetParametersAsync(parameters);
    //        if (EditContext != null && ValueExpression != null && FieldIdentifier.Model != EditContext.Model)
    //        {

    //        }

    //    }
    //}
}

