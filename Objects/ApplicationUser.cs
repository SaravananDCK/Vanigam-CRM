using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

using System.ComponentModel;
using Vanigam.CRM.Objects.Enums;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Admin : ApplicationUser
    {
        public Admin()
        {
            UserType = LoginUserType.Admin;
        }
    }
    public class SuperUser : ApplicationUser
    {
        public SuperUser()
        {
            UserType = LoginUserType.SuperUser;
        }
    }

    public class ApplicationUser : IdentityUser, IHasAudit, IHasSoftDelete
    {
        public static string SystemUserId = "84050e26-4616-4b37-a927-a16a536a5094";
        public const string SystemUserName = "System";
        public const string TenantsAdmin = nameof(TenantsAdmin);
        public const string Admin = nameof(Admin);
        public ApplicationUser()
        {
            this.CreatedAtUtc = DateTime.UtcNow;
        }
        [JsonIgnore, IgnoreDataMember]
        public override string? PasswordHash { get; set; }

        [NotMapped, IgnoreDataMember]
        public string? Password { get; set; }

        [NotMapped]
        public string? ConfirmPassword { get; set; }

        [JsonIgnore, IgnoreDataMember, NotMapped]
        public string? Name
        {
            get => UserName;
            set => UserName = value;
        }
        public string SessionId { get; set; }
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string? Avatar { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LoginUserType? UserType { get; set; } = LoginUserType.SuperUser;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LayoutMode LayoutMode { get; set; }

        public bool IsDefaultAdmin { get; set; } = false;
        public bool IsOnline { get; set; } = false;
        [ForeignKey(nameof(ApplicationTenant))]
        public int? TenantId { get; set; }
        public ApplicationTenant? ApplicationTenant { get; set; }

      
        public bool ChangePasswordOnLogon { get; set; }

        //IHasAudit
        public string? CreatedByUserId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public string? UpdatedByUserId { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        
        public string? CreatedByUserName { get; set; }
       
        public string? UpdatedByUserName { get; set; }
        
        public string? CreatedAtString { get; set; }
       
        public string? UpdatedAtString { get; set; }

        [ForeignKey(nameof(PreferredLanguage))]
        public Guid? PreferredLanguageId { get; set; }
        public Language? PreferredLanguage { get; set; }

        public Guid? ApiAccountId { get; set; }
        public string? ApiAuthToken { get; set; }

        public ICollection<ApplicationRole> Roles { get; set; } = [];
        public ICollection<ApplicationTenantUser> ApplicationTenantUsers { get; } = [];

        //IHasSoftDelete
        public bool IsNotDeleted { get; set; } = true;


    }
}

