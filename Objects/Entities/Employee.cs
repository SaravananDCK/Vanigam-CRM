using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public abstract class Employee : BaseClass
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(200)] public string? Email { get; set; }
        [StringLength(20)] public string? Phone { get; set; }
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
    }
}
