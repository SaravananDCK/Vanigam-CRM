using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Radzen;
using Vanigam.CRM.AI.Objects.Converters;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class PropertyExtensions
    {
        public static string GetOdataPropertyFullPath<T, R>(Expression<Func<T, R>> expression)
        {
            return GetPropertyFullPath(expression, '/');
        }
        public static string GetPropertyFullPath<T, R>(Expression<Func<T, R>> expression,char joinCharacter='.')
        {
            var buffer = new List<string>();
            var memberExpresiion = expression.Body as MemberExpression;
            while (memberExpresiion != null)
            {
                buffer.Add(memberExpresiion.Member.Name);
                if (memberExpresiion.Expression as MemberExpression == null)
                {
                    buffer.Add(memberExpresiion.Member.DeclaringType.Name);
                    break;
                }
                memberExpresiion = memberExpresiion.Expression as MemberExpression;
            }
            buffer.Reverse();
            if (buffer.Count > 0) buffer.RemoveAt(0);
            var data = string.Join(joinCharacter, buffer);
            return data;
        }
        public static string GetPath<T, R>(this Type type, Expression<Func<T, R>> expression)
        {
            var buffer = new List<string>();
            var memberExpression = expression.Body as MemberExpression;
            while (memberExpression != null)
            {
                buffer.Add(memberExpression.Member.Name);
                if (memberExpression.Expression as MemberExpression == null)
                {
                    buffer.Add(memberExpression.Member.DeclaringType.Name);
                    break;
                }
                memberExpression = memberExpression.Expression as MemberExpression;
            }
            buffer.Reverse();
            if (buffer.Count > 0) buffer.RemoveAt(0);
            return string.Join('/', buffer);
        }
    }
    public static class VanigamAccountingODataJsonSerializer
    {
        /// <summary>Determines whether the specified type is complex.</summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is complex; otherwise, <c>false</c>.</returns>
        private static bool IsComplex(Type type)
        {
            Type type1 = !type.IsGenericType || !(type.GetGenericTypeDefinition() == typeof(Nullable<>)) ? type : Nullable.GetUnderlyingType(type);
            Type c = type1.IsGenericType ? ((IEnumerable<Type>)type1.GetGenericArguments()).FirstOrDefault<Type>() : type1;
            return !c.IsPrimitive && !typeof(IEnumerable<>).IsAssignableFrom(c) && type != typeof(string) && type != typeof(Decimal) && type.IsClass;
        }

        /// <summary>Determines whether the specified type is enumerable.</summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is enumerable; otherwise, <c>false</c>.</returns>
        private static bool IsEnumerable(Type type)
        {
            if (typeof(string).IsAssignableFrom(type))
                return false;
            return typeof(IEnumerable<>).IsAssignableFrom(type) || typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>Serializes the specified value.</summary>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        /// <returns>System.String.</returns>
        public static string Serialize<TValue>(TValue value, JsonSerializerOptions options = null)
        {
            if (options == null)
            {
                options = new JsonSerializerOptions();
                options.Converters.Add(new ByteArrayConverter());
            }
            IEnumerable<PropertyInfo> source1 = ((IEnumerable<PropertyInfo>)typeof(TValue).GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>)(p => VanigamAccountingODataJsonSerializer.IsComplex(p.PropertyType) || VanigamAccountingODataJsonSerializer.IsEnumerable(p.PropertyType)));
            IEnumerable<PropertyInfo> source2 = ((IEnumerable<PropertyInfo>)typeof(TValue).GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>)(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)));
            if (source1.Any<PropertyInfo>() || source2.Any<PropertyInfo>())
                options.Converters.Add((JsonConverter)new ComplexPropertiesConverter<TValue>(source1.Select<PropertyInfo, string>((Func<PropertyInfo, string>)(p => p.Name))));
            return JsonSerializer.Serialize<TValue>(value, options);
        }
    }
}

