using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Sla : BaseClass
    {
        public string Name { get; set; } = string.Empty;
        public int ResponseHours { get; set; }
        public int ResolutionHours { get; set; }
    }
}
