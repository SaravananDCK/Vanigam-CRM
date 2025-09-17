using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class ServiceExtensions
    {
        public static void AddInheritedClasses(this IServiceCollection serviceCollection , Type myType)
        {
            var services = GetInheritedClasses(myType.Assembly,myType);
            foreach (var service in services)
            {
                serviceCollection.AddScoped(service);
            }
        }
        public  static IEnumerable<Type> GetInheritedClasses(this Assembly assembly,Type myType)
        {
            //if you want the abstract classes drop the !TheType.IsAbstract but it is probably to instance so its a good idea to keep it.
            var types = assembly.GetTypes();
            return types.Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                                    t.BaseType.GetGenericTypeDefinition() == myType);
        }
    }
}

