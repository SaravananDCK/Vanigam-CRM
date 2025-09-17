using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects
{
    public static class VersionInfo
    {
        public const string Value = "25.1.1.0";

        public static System.Version Version = new System.Version(Value);
        public static System.Version GetVersion()
        {
            return new System.Version(Value);
        }
    }
}

