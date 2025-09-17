using System.Linq.Expressions;

namespace Vanigam.CRM.Objects.OData
{
    public class ODataExpand<T>
    {
        private readonly List<string> _filters = new List<string>();
        private string _operator = string.Empty;
        public ODataExpand<T> Expand(Expression<Func<T, object>> predicate)
        {
            var filterString = ConvertExpressionToODataFilter(predicate);
            if (filterString.Contains("/"))
            {
                var values = filterString.Split("/");
                filterString = $"{values[0]}($expand={values[1]})";
            }
            _filters.Add($"{_operator} {filterString}");
            _operator = ",";
            return this;
        }
        public ODataExpand<T> Expand(string filterString)
        {
            _filters.Add($"{_operator} {filterString}");
            _operator = ",";
            return this;
        }
        public ODataExpand<T> Expand(Expression<Func<T, object>> predicate,params Expression<Func<T, object>>[] selectPredicates)
        {
            string selectString = string.Empty;
            foreach (var expression in selectPredicates)
            {
                var select= ConvertExpressionToODataSelectFilter(expression);
                if (selectString == string.Empty)
                {
                    selectString = select;
                }
                else
                {
                    selectString += "," + select;
                }
            }
            var filterString = ConvertExpressionToODataFilter(predicate);
            if (filterString.Contains("/"))
            {
                var values = filterString.Split("/");
                filterString = $"{values[0]}($select={values[1]};$expand={values[1]}($select={selectString}))";
                _filters.Add($"{_operator} {filterString}");
            }
            else
            {
                _filters.Add($"{_operator} {filterString}($select={selectString})");
            }
            _operator = ",";
            return this;
        }
        private string ConvertExpressionToODataSelectFilter(Expression<Func<T, object>> predicate)
        {
            // This will be a simple implementation that only handles 'Equal' binary expressions
            if (predicate.Body is MemberExpression memberExpression)
            {
               return memberExpression.Member.Name; ;
            }
            else if (predicate.Body is UnaryExpression unaryExpression)
            {
               return ((MemberExpression)unaryExpression.Operand).Member.Name;
            }
            throw new NotSupportedException("This expression type is not supported yet.");
        }
        private string ConvertExpressionToODataFilter(Expression<Func<T, object>> predicate)
        {
            // This will be a simple implementation that only handles 'Equal' binary expressions
            if (predicate.Body is MemberExpression memberExpression)
            {
               return GetFullPropertyPath(memberExpression); ;
            }
            else if (predicate.Body is UnaryExpression unaryExpression)
            {
               return ((MemberExpression)unaryExpression.Operand).Member.Name;
            }
            throw new NotSupportedException("This expression type is not supported yet.");
        }
        private string GetFullPropertyPath(MemberExpression expression)
        {
            // Recursively get the full property path for nested properties
            var propertyPath = new List<string>();
            while (expression != null)
            {
                propertyPath.Insert(0, expression.Member.Name);
                expression = expression.Expression as MemberExpression;
            }
            return string.Join("/", propertyPath);
        }
        public string Build()
        {
            return string.Join(string.Empty, _filters);
        }
    }

}

