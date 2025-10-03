using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class AccountGroup : BaseClass
{
    [StringLength(100)]
    public virtual string Name { get; set; } = "";

    public virtual Guid? ParentGroupId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public virtual AccountNature Nature { get; set; }

    [StringLength(20)]
    public virtual string? Code { get; set; }

    public virtual bool IsActive { get; set; } = true;

    public virtual AccountGroup ParentGroup { get; set; }
    public virtual ICollection<AccountGroup> ChildGroups { get; set; } = new ObservableCollection<AccountGroup>();
    public virtual ICollection<LedgerAccount> LedgerAccounts { get; set; } = new ObservableCollection<LedgerAccount>();
}