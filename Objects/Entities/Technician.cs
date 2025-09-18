using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Technician : ApplicationUser
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TechnicianStatus Status { get; set; } = TechnicianStatus.Offline;

        public ICollection<JobAssignment> Assignments { get; set; } = new List<JobAssignment>();
        public ICollection<TimeSheet> Timesheets { get; set; } = new List<TimeSheet>();
    }
}
