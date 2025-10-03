using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities;

public class TaxCode : BaseClass
{
    [StringLength(20)]
    [DisplayName("Tax Code")]
    public virtual string Code { get; set; } = "";

        
    [StringLength(100)]
    [DisplayName("Tax Name")]
    public virtual string Name { get; set; } = "";

    [StringLength(500)]
    [DisplayName("Description")]
    public virtual string? Description { get; set; }

    [DisplayName("Tax Type")]
    [StringLength(20)]
    public virtual string TaxType { get; set; } = "GST"; // GST, VAT, Service Tax, etc.

    [Range(0, 100)]
    [DisplayName("Tax Rate (%)")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public virtual decimal TaxRate { get; set; }

    [DisplayName("Is Compound Tax")]
    public virtual bool IsCompoundTax { get; set; } = false;

    [DisplayName("Is Active")]
    public virtual bool IsActive { get; set; } = true;

    [DisplayName("Effective From")]
    [DataType(DataType.Date)]
    public virtual DateTime? EffectiveFrom { get; set; } 

    [DisplayName("Effective To")]
    [DataType(DataType.Date)]
    public virtual DateTime? EffectiveTo { get; set; }

    // GST Specific Fields
    [DisplayName("CGST Rate (%)")]
    [Range(0, 100)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public virtual decimal CGSTRate { get; set; }

    [DisplayName("SGST Rate (%)")]
    [Range(0, 100)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public virtual decimal SGSTRate { get; set; }

    [DisplayName("IGST Rate (%)")]
    [Range(0, 100)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public virtual decimal IGSTRate { get; set; }

    [DisplayName("UTGST Rate (%)")]
    [Range(0, 100)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public virtual decimal UTGSTRate { get; set; }

    [DisplayName("Cess Rate (%)")]
    [Range(0, 100)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public virtual decimal CessRate { get; set; }

    // Account Mapping for Tax Posting
    [DisplayName("Tax Payable Account")]
    public virtual Guid? TaxPayableAccountId { get; set; }

    [DisplayName("Tax Receivable Account")]
    public virtual Guid? TaxReceivableAccountId { get; set; }

    // Navigation properties
        
    public virtual LedgerAccount TaxPayableAccount { get; set; }
    public virtual LedgerAccount TaxReceivableAccount { get; set; }
    public virtual ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
}