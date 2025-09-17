using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.ReportParameter
{
    public abstract class ReportParameter
    {
        public int? TenantId { get; set; }
        public Guid? CurrentObjectId { get; set; }
        public abstract string GetCriteria();
        public abstract string GetReportName();
        public abstract string GetReportTitle();
        public abstract string GetReportCategory();
    }
}

