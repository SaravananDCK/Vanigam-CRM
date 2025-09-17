
namespace Vanigam.CRM.Objects.Entities;

public class Language : CodedClass,ISequence
{
    public static Guid EnglishOid = Guid.Parse("472b3a8c-1762-429e-9322-f3c19cfc1000");
    public static Guid TamilOid = Guid.Parse("60c9157d-ed0f-42f5-92c4-f36c727432f2");
    public int? Sequence { get; set; }
}
