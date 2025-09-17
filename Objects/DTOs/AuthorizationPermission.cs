using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.DTOs
{
    public class AuthorizationPermission
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ClaimType { get; set; }
        public string Role { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string RoleId { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public RolePriority Priority { get; set; }
    }
    
    public enum RolePriority
    {
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5,
        Sixth = 6,
        Seventh = 7,
        Eighth = 8,
        Ninth = 9,
        Tenth = 10,
        Eleventh = 11
    }
}

