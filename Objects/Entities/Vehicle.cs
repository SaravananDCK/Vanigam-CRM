using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Vehicle : BaseClass
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string? Make { get; set; }
        public string? Model { get; set; }
        public Guid? LocationId { get; set; }
    }
}
