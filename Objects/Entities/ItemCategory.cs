using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class ItemCategory : NamedClass
    {
        public bool IsActive { get; set; } = true;
    }
}
