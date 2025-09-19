using System.Reflection;
using System.Collections;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Client.Services;

public interface IEntityCollectionDiscoveryService
{
    List<CollectionPropertyInfo> GetCollectionProperties<T>() where T : class;
    List<CollectionPropertyInfo> GetCollectionProperties(Type entityType);
}

public class CollectionPropertyInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public Type ChildEntityType { get; set; } = typeof(object);
    public string DisplayName { get; set; } = string.Empty;
    public string ForeignKeyPropertyName { get; set; } = string.Empty;
    public PropertyInfo PropertyInfo { get; set; } = null!;
}

public class EntityCollectionDiscoveryService : IEntityCollectionDiscoveryService
{
    public List<CollectionPropertyInfo> GetCollectionProperties<T>() where T : class
    {
        return GetCollectionProperties(typeof(T));
    }

    public List<CollectionPropertyInfo> GetCollectionProperties(Type entityType)
    {
        var collectionProperties = new List<CollectionPropertyInfo>();

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => IsCollectionProperty(p))
            .ToList();

        foreach (var property in properties)
        {
            var childEntityType = GetCollectionElementType(property.PropertyType);
            if (childEntityType != null && typeof(IHasId<Guid>).IsAssignableFrom(childEntityType))
            {
                var foreignKeyProperty = GetForeignKeyPropertyName(entityType, childEntityType, property.Name);

                collectionProperties.Add(new CollectionPropertyInfo
                {
                    PropertyName = property.Name,
                    ChildEntityType = childEntityType,
                    DisplayName = FormatDisplayName(property.Name),
                    ForeignKeyPropertyName = foreignKeyProperty,
                    PropertyInfo = property
                });
            }
        }

        return collectionProperties;
    }

    private bool IsCollectionProperty(PropertyInfo property)
    {
        if (property.PropertyType == typeof(string))
            return false;

        return typeof(IEnumerable).IsAssignableFrom(property.PropertyType) &&
               property.PropertyType.IsGenericType;
    }

    private Type? GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length == 1)
                return genericArgs[0];
        }

        return null;
    }

    private string GetForeignKeyPropertyName(Type parentType, Type childType, string collectionPropertyName)
    {
        // Convention: {ParentTypeName}Id
        var conventionName = $"{parentType.Name}Id";

        var childProperties = childType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // First try the convention
        var conventionProperty = childProperties.FirstOrDefault(p =>
            string.Equals(p.Name, conventionName, StringComparison.OrdinalIgnoreCase));

        if (conventionProperty != null)
            return conventionProperty.Name;

        // Try to find by ForeignKey attribute
        var foreignKeyProperty = childProperties.FirstOrDefault(p =>
        {
            var foreignKeyAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>();
            return foreignKeyAttr != null;
        });

        if (foreignKeyProperty != null)
            return foreignKeyProperty.Name;

        // Fallback to convention
        return conventionName;
    }

    private string FormatDisplayName(string propertyName)
    {
        // Convert PascalCase to spaced words
        return System.Text.RegularExpressions.Regex.Replace(propertyName, "([A-Z])", " $1").Trim();
    }
}