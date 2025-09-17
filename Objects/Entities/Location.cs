using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Location : BaseClass
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
