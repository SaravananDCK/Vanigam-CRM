using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.Exceptions
{
    public class NotUniqueException : Exception
    {
        public Type Type { get; set; }

        public NotUniqueException(Type type, string message) : base(message)
        {
            Type = type;
        }
    }
}

