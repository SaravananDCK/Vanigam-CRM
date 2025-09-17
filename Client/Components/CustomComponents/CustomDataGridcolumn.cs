using System.Collections;
using System.Globalization;
using System.Linq.Dynamic.Core;
using Radzen;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class CustomDataGridColumn<TItem> : RadzenDataGridColumn<TItem>
    {
        internal static readonly IDictionary<FilterOperator, string> ODataFilterOperators = new Dictionary<FilterOperator, string>
        {
            {FilterOperator.Equals, "eq"},
            {FilterOperator.NotEquals, "ne"},
            {FilterOperator.LessThan, "lt"},
            {FilterOperator.LessThanOrEquals, "le"},
            {FilterOperator.GreaterThan, "gt"},
            {FilterOperator.GreaterThanOrEquals, "ge"},
            {FilterOperator.StartsWith, "startswith"},
            {FilterOperator.EndsWith, "endswith"},
            {FilterOperator.Contains, "contains"},
            {FilterOperator.DoesNotContain, "DoesNotContain"},
            {FilterOperator.IsNull, "eq"},
            {FilterOperator.IsEmpty, "eq"},
            {FilterOperator.IsNotNull, "ne"},
            {FilterOperator.IsNotEmpty, "ne"},
            {FilterOperator.In, "in"},
            {FilterOperator.NotIn, "in"},
            {FilterOperator.Custom, ""}
        };
        bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) || typeof(IEnumerable<>).IsAssignableFrom(type);
        }
        bool IsEnum(Type source)
        {
            if (source == null)
                return false;
            return source.IsEnum;
        }
        bool IsNullableEnum(Type elementType)
        {
           return Nullable.GetUnderlyingType(elementType).IsEnum && Nullable.GetUnderlyingType(elementType) != null;
        }
        protected override string GetColumnODataFilter(object filterValue, FilterOperator columnFilterOperator)
        {
            var column = this;
            var property = column.GetFilterProperty().Replace('.', '/');

            var odataFilterOperator = ODataFilterOperators[columnFilterOperator];

            var value = IsEnumerable(column.FilterPropertyType) && column.FilterPropertyType != typeof(string)
                ? null
                : (string)Convert.ChangeType(filterValue is DateTimeOffset ?
                            ((DateTimeOffset)filterValue).UtcDateTime : filterValue is DateOnly ?
                                ((DateOnly)filterValue).ToString("yyy-MM-dd", CultureInfo.InvariantCulture) :
                                    filterValue, typeof(string), CultureInfo.InvariantCulture);

            if (column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive && column.FilterPropertyType == typeof(string))
            {
                property = $"tolower({property})";
            }

            if (PropertyAccess.IsEnum(column.FilterPropertyType) || PropertyAccess.IsNullableEnum(column.FilterPropertyType))
            {
                return $"{property} {odataFilterOperator} '{value}'";
            }
            else if (column.FilterPropertyType == typeof(string))
            {
                if (!string.IsNullOrEmpty(value) && columnFilterOperator == FilterOperator.Contains)
                {
                    return column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive ?
                        $"contains({property}, tolower('{value}'))" :
                        $"contains({property}, '{value}')";
                }
                else if (!string.IsNullOrEmpty(value) && columnFilterOperator == FilterOperator.DoesNotContain)
                {
                    return column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive ?
                        $"not(contains({property}, tolower('{value}')))" :
                        $"not(contains({property}, '{value}'))";
                }
                else if (!string.IsNullOrEmpty(value) && columnFilterOperator == FilterOperator.StartsWith)
                {
                    return column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive ?
                        $"startswith({property}, tolower('{value}'))" :
                        $"startswith({property}, '{value}')";
                }
                else if (!string.IsNullOrEmpty(value) && columnFilterOperator == FilterOperator.EndsWith)
                {
                    return column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive ?
                        $"endswith({property}, tolower('{value}'))" :
                        $"endswith({property}, '{value}')";
                }
                else if (!string.IsNullOrEmpty(value) && columnFilterOperator == FilterOperator.Equals)
                {
                    return column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive ?
                        $"{property} eq tolower('{value}')" :
                        $"{property} eq '{value}'";
                }
                else if (!string.IsNullOrEmpty(value) && columnFilterOperator == FilterOperator.NotEquals)
                {
                    return column.Grid.FilterCaseSensitivity == FilterCaseSensitivity.CaseInsensitive ?
                        $"{property} ne tolower('{value}')" :
                        $"{property} ne '{value}'";
                }
                else if (columnFilterOperator == FilterOperator.IsNull || columnFilterOperator == FilterOperator.IsNotNull)
                {
                    return $"{property} {odataFilterOperator} null";
                }
                else if (columnFilterOperator == FilterOperator.IsEmpty || columnFilterOperator == FilterOperator.IsNotEmpty)
                {
                    return $"{property} {odataFilterOperator} ''";
                }
            }
            else if (IsEnumerable(column.FilterPropertyType) && column.FilterPropertyType != typeof(string))
            {
                var enumerableValue = ((IEnumerable)(filterValue != null ? filterValue : Enumerable.Empty<object>())).AsQueryable();
                var elementType = enumerableValue.ElementType;
                string enumerableValueAsString = "";
                string enumerableValueAsStringOrForAny = "";
                if (IsEnum(elementType))
                {
                    enumerableValueAsString = "(" + String.Join(",", (enumerableValue.Cast<int>().Select(i => $@"'{i}'").Cast<object>())) + ")";
                }
                else if (IsNullableEnum(elementType))
                {
                    enumerableValueAsString = "(" +String.Join(",", (enumerableValue.OfType<Enum?>().Select(i => $@"'{i}'").Cast<object>())) + ")";
                }
                else
                {

                    enumerableValueAsString = "(" + String.Join(",",
                        (enumerableValue.ElementType == typeof(string) ? enumerableValue.Cast<string>().Select(i => $@"'{i}'").Cast<object>() : enumerableValue.Cast<object>())) + ")";
                }


                enumerableValueAsStringOrForAny = String.Join(" or ",
                    (enumerableValue.ElementType == typeof(string) ? enumerableValue.Cast<string>()
                        .Select(i => $@"i/{property} eq '{i}'").Cast<object>() : enumerableValue.Cast<object>().Select(i => $@"i/{property} eq {i}").Cast<object>()));

                if (enumerableValue.Any() && columnFilterOperator == FilterOperator.Equals)
                {
                    return $"{property} in {enumerableValueAsString}";
                }
                else if (enumerableValue.Any() && columnFilterOperator == FilterOperator.Contains)
                {
                    return $"{property} in {enumerableValueAsString}";
                }
                else if (enumerableValue.Any() && columnFilterOperator == FilterOperator.DoesNotContain)
                {
                    return $"not({property} in {enumerableValueAsString})";
                }
                else if (enumerableValue.Any() && columnFilterOperator == FilterOperator.In)
                {
                    return $"{column.Property}/any(i:{enumerableValueAsStringOrForAny})";
                }
                else if (enumerableValue.Any() && columnFilterOperator == FilterOperator.NotIn)
                {
                    return $"not({column.Property}/any(i: {enumerableValueAsStringOrForAny}))";
                }
            }
            else if (PropertyAccess.IsNumeric(column.FilterPropertyType))
            {
                if (columnFilterOperator == FilterOperator.IsNull || columnFilterOperator == FilterOperator.IsNotNull)
                {
                    return $"{property} {odataFilterOperator} null";
                }
                else
                {
                    return $"{property} {odataFilterOperator} {value}";
                }
            }
            else if (column.FilterPropertyType == typeof(bool) || column.FilterPropertyType == typeof(bool?))
            {
                if (columnFilterOperator == FilterOperator.IsNull || columnFilterOperator == FilterOperator.IsNotNull)
                {
                    return $"{property} {odataFilterOperator} null";
                }
                else if (columnFilterOperator == FilterOperator.IsEmpty || columnFilterOperator == FilterOperator.IsNotEmpty)
                {
                    return $"{property} {odataFilterOperator} ''";
                }
                else
                {
                    return $"{property} eq {value.ToLower()}";
                }
            }
            else if (column.FilterPropertyType == typeof(DateTime) ||
                    column.FilterPropertyType == typeof(DateTime?) ||
                    column.FilterPropertyType == typeof(DateTimeOffset) ||
                    column.FilterPropertyType == typeof(DateTimeOffset?) ||
                    column.FilterPropertyType == typeof(DateOnly) ||
                    column.FilterPropertyType == typeof(DateOnly?))
            {
                if (columnFilterOperator == FilterOperator.IsNull || columnFilterOperator == FilterOperator.IsNotNull)
                {
                    return $"{property} {odataFilterOperator} null";
                }
                else if (columnFilterOperator == FilterOperator.IsEmpty || columnFilterOperator == FilterOperator.IsNotEmpty)
                {
                    return $"{property} {odataFilterOperator} ''";
                }
                else
                {
                    return $"{property} {odataFilterOperator} {(column.FilterPropertyType == typeof(DateOnly) || column.FilterPropertyType == typeof(DateOnly?) ? value : DateTime.Parse(value, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture))}";
                }
            }
            else if (column.FilterPropertyType == typeof(Guid) || column.FilterPropertyType == typeof(Guid?))
            {
                return $"{property} {odataFilterOperator} {value}";
            }

            return "";
        }
    }
}

