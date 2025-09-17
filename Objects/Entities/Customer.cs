using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Customer : BaseClass
    {
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
