using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.OData
{
    public class ODataFilter<T>
    {
        private readonly List<string> _filters = new List<string>();
        private string _logicalOperator = string.Empty;
        private bool _useGrouping = false;

        public ODataFilter<T> BeginGroup()
        {
            _filters.Add($"{_logicalOperator} ("); // Start a new group
            _logicalOperator = string.Empty; // Reset after use
            _useGrouping = true;
            return this;
        }

        public ODataFilter<T> EndGroup()
        {
            _filters.Add(")"); // End the current group
            _useGrouping = false;
            return this;
        }

        public ODataFilter<T> FilterBy(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                _filters.Add(filter);
            }
            return this;
        }
        public ODataFilter<T> FilterByAnd(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                _filters.Add(filter);
                _filters.Add(" and ");
            }
            return this;
        }
        public ODataFilter<T> FilterByOr(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                _filters.Add(filter);
                _filters.Add(" or ");
            }
            return this;
        }

        public ODataFilter<T> FilterByAnd(Expression<Func<T, bool>> predicate)
        {
            return FilterBy(predicate).And();
        }
        public ODataFilter<T> FilterByAnd(Expression<Func<T, bool>> predicate,bool criteria)
        {
            if (criteria)
            {
               return FilterBy(predicate).And();
            }

            return this;
        }
        public ODataFilter<T> FilterByOr(Expression<Func<T, bool>> predicate)
        {
            return FilterBy(predicate).Or();
        }
        public ODataFilter<T> FilterByOr(Expression<Func<T, bool>> predicate, bool criteria)
        {
            if (criteria)
            {
                return FilterBy(predicate).Or();
            }

            return this;
        }
        public ODataFilter<T> ContainsAnd(Expression<Func<T, string>> predicate, string value, bool criteria)
        {
            if (criteria)
            {
                return Contains(predicate, value).And();
            }
            return this;
        }
        public ODataFilter<T> ContainsAnd(Expression<Func<T, string>> predicate, string value)
        {
            return Contains(predicate, value).And();
        }
        public ODataFilter<T> ContainsOr(Expression<Func<T, string>> predicate, string value)
        {
            return Contains(predicate, value).Or();
        }
        public ODataFilter<T> ContainsOr(Expression<Func<T, Guid>> predicate, Guid value)
        {
            return Contains(predicate, value).Or();
        }
        public ODataFilter<T> ContainsOr(Expression<Func<T, string>> predicate, string value, bool criteria)
        {
            if (criteria)
            {
                return Contains(predicate, value).Or();
            }
            return this;
        }
        public ODataFilter<T> FilterBy(Expression<Func<T, bool>> predicate)
        {
            var filterString = ConvertExpressionToODataFilter(predicate);
            if (_useGrouping && _filters.Last() == "(")
            {
                _filters.Add(filterString); // Add filter string inside the group
            }
            else
            {
                _filters.Add($"{_logicalOperator}{filterString}");
            }
            _logicalOperator = string.Empty; // Reset after use
            return this;
        }

        public ODataFilter<T> And()
        {
            _logicalOperator = " and ";
            return this;
        }

        public ODataFilter<T> Or()
        {
            _logicalOperator = " or ";
            return this;
        }

        public string Build()
        {
            return string.Join(string.Empty, _filters);
        }
        public ODataFilter<T> Contains(Expression<Func<T, Guid>> predicate, Guid value)
        {
            // Convert the expression into an OData 'contains' function
            if (value!=Guid.Empty)
            {
                var propertyName = string.Empty;
                if (predicate.Body is MemberExpression memberExpression)
                {
                    propertyName = GetFullPropertyPath(memberExpression);
                }

                //var filterString = ConvertExpressionToODataFunction(predicate, "contains", value);
                    _filters.Add($"{_logicalOperator} {propertyName} eq {value}");
            }
            else
            {
                _filters.Add($"{_logicalOperator} true");
            }
            _logicalOperator = string.Empty; // Reset after use
            return this;
        }
        public ODataFilter<T> Contains(Expression<Func<T, string>> predicate, string value)
        {
            // Convert the expression into an OData 'contains' function
            if (!string.IsNullOrEmpty(value))
            {
                var filterString = ConvertExpressionToODataFunction(predicate, "contains", value);
                _filters.Add($"{_logicalOperator}{filterString}");
            }
            else
            {
                _filters.Add($"{_logicalOperator} true");
            }
            _logicalOperator = string.Empty; // Reset after use
            return this;
        }
        public ODataFilter<T> StartsWith(Expression<Func<T, string>> predicate, string value)
        {
            // Convert the expression into an OData 'startsWith' function
            if (!string.IsNullOrEmpty(value))
            {
                var filterString = ConvertExpressionToODataFunction(predicate, "startsWith", value);
                _filters.Add($"{_logicalOperator}{filterString}");
            }
            _logicalOperator = string.Empty; // Reset after use
            return this;
        }

        private string ConvertExpressionToODataFunction(Expression<Func<T, string>> predicate, string functionName, string value)
        {
            if (predicate.Body is MemberExpression memberExpression)
            {
                var propertyName = GetFullPropertyPath(memberExpression);
                // Construct the OData function string
                return $"({functionName}(tolower({propertyName}),tolower(\"{value}\")))";
                //return $"{functionName}({propertyName}, '{value}')";
            }

            throw new NotSupportedException("This expression type is not supported yet.");
        }
        private string ConvertExpressionToODataFilter(Expression<Func<T, bool>> predicate)
        {
            if (predicate.Body is BinaryExpression binaryExpression)
            {
                return ConvertBinaryExpressionToOData(binaryExpression);
            }

            throw new NotSupportedException("This expression type is not supported yet.");
        }

        private string ConvertBinaryExpressionToOData(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse)
            {
                string left = ConvertBinaryExpressionToOData((BinaryExpression)binaryExpression.Left);
                string right = ConvertBinaryExpressionToOData((BinaryExpression)binaryExpression.Right);

                string logicalOperator = binaryExpression.NodeType == ExpressionType.AndAlso ? " and " : " or ";
                return $"({left}{logicalOperator}{right})";
            }
            else if (binaryExpression.NodeType == ExpressionType.Equal)
            {
                // Assuming the left side of the equation is a member expression (e.g., p.Name)
                if (GetPropertyQuery(binaryExpression, "eq", out var s))
                    return s;
            }
            else if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                // Assuming the left side of the equation is a member expression (e.g., p.Name)
                if (GetPropertyQuery(binaryExpression, "ge", out var s))
                    return s;
            }
            else if (binaryExpression.NodeType == ExpressionType.GreaterThan)
            {
                // Assuming the left side of the equation is a member expression (e.g., p.Name)
                if (GetPropertyQuery(binaryExpression, "gt", out var s))
                    return s;
            }
            else if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual)
            {
                // Assuming the left side of the equation is a member expression (e.g., p.Name)
                if (GetPropertyQuery(binaryExpression, "le", out var s))
                    return s;
            }
            else if (binaryExpression.NodeType == ExpressionType.LessThan)
            {
                // Assuming the left side of the equation is a member expression (e.g., p.Name)
                if (GetPropertyQuery(binaryExpression, "lt", out var s))
                    return s;
            }
            else if (binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                // Assuming the left side of the equation is a member expression (e.g., p.Name)
                if (GetPropertyQuery(binaryExpression, "ne", out var s))
                    return s;
            }
            // Add support for other binary expression types as needed

            throw new NotSupportedException("This binary expression type is not supported yet.");
        }
        //private string ConvertExpressionToODataFilter(Expression<Func<T, bool>> predicate)
        //{
        //    // This will be a simple implementation that only handles 'Equal' binary expressions
        //    if (predicate.Body is BinaryExpression binaryExpression)
        //    {
        //        if (binaryExpression.NodeType == ExpressionType.Equal)
        //        {
        //            // Assuming the left side of the equation is a member expression (e.g., p.Name)
        //            if (GetPropertyQuery(binaryExpression, "eq", out var s))
        //                return s;
        //        }
        //        else if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual)
        //        {
        //            // Assuming the left side of the equation is a member expression (e.g., p.Name)
        //            if (GetPropertyQuery(binaryExpression, "ge", out var s))
        //                return s;
        //        }
        //        else if (binaryExpression.NodeType == ExpressionType.GreaterThan)
        //        {
        //            // Assuming the left side of the equation is a member expression (e.g., p.Name)
        //            if (GetPropertyQuery(binaryExpression, "gt", out var s))
        //                return s;
        //        }
        //        else if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual)
        //        {
        //            // Assuming the left side of the equation is a member expression (e.g., p.Name)
        //            if (GetPropertyQuery(binaryExpression, "le", out var s))
        //                return s;
        //        }
        //        else if (binaryExpression.NodeType == ExpressionType.LessThan)
        //        {
        //            // Assuming the left side of the equation is a member expression (e.g., p.Name)
        //            if (GetPropertyQuery(binaryExpression, "lt", out var s))
        //                return s;
        //        }
        //        else if (binaryExpression.NodeType == ExpressionType.NotEqual)
        //        {
        //            // Assuming the left side of the equation is a member expression (e.g., p.Name)
        //            if (GetPropertyQuery(binaryExpression, "ne", out var s))
        //                return s;
        //        }
        //    }
        //    //else if (predicate.Body is MemberExpression memberExpression)
        //    //{
        //    //    // Check if the member is a property of a navigation property
        //    //    if (memberExpression.Expression is MemberExpression parentMemberExpression)
        //    //    {
        //    //        // Construct the OData filter string with navigation property
        //    //        return $"{parentMemberExpression.Member.Name}/{memberExpression.Member.Name} eq '{GetConstantValue(memberExpression)}'";
        //    //    }
        //    //    else
        //    //    {
        //    //        // Handle the simple case where there is no navigation property
        //    //        return $"{memberExpression.Member.Name} eq '{GetConstantValue(memberExpression)}'";
        //    //    }
        //    //}

        //    throw new NotSupportedException("This expression type is not supported yet.");
        //}

        private bool GetPropertyQuery(BinaryExpression binaryExpression, string operatorString, out string s)
        {
            //if (binaryExpression.Left is UnaryExpression unaryExpression)
            //{

            //} else 
            if (binaryExpression.Left is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand is MemberExpression memberExpression1)
                {
                    return GetPropertyQuery(binaryExpression, operatorString, out s, memberExpression1);
                }
            }
            if (binaryExpression.Left is MemberExpression memberExpression)
            {
                //if (IsNullableType(memberExpression.Type))
                //{
                //    memberExpression = Expression.Property(memberExpression, "Value");
                //}
                return GetPropertyQuery(binaryExpression, operatorString, out s, memberExpression);
            }

            s = "";
            return false;
        }

        private bool GetPropertyQuery(BinaryExpression binaryExpression, string operatorString, out string s,
            MemberExpression memberExpression)
        {
            string propertyName = GetFullPropertyPath(memberExpression);
            string value = GetConstantExpressionValue(binaryExpression.Right);
            if (value == "null")
            {
                {
                    s = $"{propertyName} {operatorString} null";
                    return true;
                }
            }
            else if (value == "True"|| value == "False")
            {
                {
                    s = $"{propertyName} {operatorString} {value.ToLower()}";
                    return true;
                }
            }
            else
            {
                // Construct the OData filter string
                {
                    s = $"{propertyName} {operatorString} {value}";
                    return true;
                }
            }
        }

        private bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
        private string GetConstantExpressionValue(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
            {
                return GetFormattedValue(constantExpression.Value);
            }
            else if (expression is UnaryExpression unaryExpression && IsNullableType(unaryExpression.Type))
            {
                // If the member is a nullable type, we need to handle it accordingly
                var objectMember = Expression.Convert(unaryExpression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                return GetFormattedValue(getter());

            }
            else if (expression is MemberExpression memberExpression && IsNullableType(memberExpression.Type))
            {
                // If the member is a nullable type, we need to handle it accordingly
                var objectMember = Expression.Convert(memberExpression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                object value = getter();
                if (value == null)
                {
                    return "null";
                }
                else
                {
                    // Format the value based on its type, e.g., DateTime
                    return value.ToString();
                }
            }
            // Handle other expression types as needed
            throw new NotImplementedException("Expression type not supported yet.");
        }
        private string GetFormattedValue(object value)
        {
            if (value == null)
            {
                return "null";
            }
            else if (value is DateTimeOffset || value is DateTimeOffset?)
            {
                DateTimeOffset date = (DateTimeOffset)value;
                return date.ToString("yyyy-MM-ddTHH:mm:ssZ"); // 'o' format for OData DateTime values
            }
            else if (value is DateTime || value is DateTime?)
            {
                DateTime date = (DateTime)value;
                return date.ToString("yyyy-MM-ddTHH:mm:ssZ"); // 'o' format for OData DateTime values
            }
            else if (value is int)
            {
                return$"'{value}'" ; // 'o' format for OData DateTime values
            }
            else
            {
                return value.ToString();
            }
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
        private object GetConstantValue(MemberExpression memberExpression)
        {
            // This method should extract the value from the right side of the expression
            // You will need to implement this based on your specific requirements
            throw new NotImplementedException();
        }
    }

}

