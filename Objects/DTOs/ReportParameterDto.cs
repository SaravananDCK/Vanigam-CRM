using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.DTOs
{
    public class ReportParameterDto
    {
        public ReportParameter.ReportParameter ReportParameter { get; }

        public ReportParameterDto()
        {

        }
        public ReportParameterDto(Guid? patientId, ReportParameter.ReportParameter? reportParameter)
        {
            PatientId = patientId;
            ReportParameter = reportParameter;
            ReportName = ReportParameter?.GetReportName();
            FileName = $"{ReportParameter?.GetReportTitle()}.pdf";
            Title = $"{ReportParameter?.GetReportTitle()}";
            Criteria = $"{ReportParameter?.GetCriteria()}";
            Category = $"{ReportParameter?.GetReportCategory()}";
        }
        public Guid? PatientId { get; set; }
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string ReportName { get; set; }
        public string Criteria { get; set; }
        public string FileName { get; set; }
        public string Category { get; set; }
        public List<ReportParameterValueDto> Parameters { get; set; } = new List<ReportParameterValueDto>();

    }

    public class ReportParameterValueDto
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}

