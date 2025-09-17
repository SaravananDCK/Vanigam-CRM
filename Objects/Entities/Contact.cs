using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Contact : BaseClass
    {
        public ContactType Type { get; set; } = ContactType.Individual;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
