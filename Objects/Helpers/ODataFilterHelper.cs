using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class ODataFilterHelper
    {
        public static string GetContainsFilter(this string propertyName, string search)
        {
            return (string.IsNullOrEmpty(search) ? "true" : $@"(contains(tolower({propertyName}),tolower(""{search}"")))");
        }
        public static string GetContainsFilterNumeric(this string propertyName, string search)
        {
            return $@"(contains(tolower({propertyName}),{search}))";
        }
        public static string AppendFilter(this string filter, string appendFilter, string prefix = "and")
        {
            if (!string.IsNullOrEmpty(filter))
            {
                filter += $" {prefix} ";
            }
            filter += appendFilter;
            return filter;
        }
    }
}

