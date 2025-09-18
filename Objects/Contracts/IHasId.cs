namespace Vanigam.CRM.Objects.Contracts
{
    public interface IHasId<T>
    {
        T Oid { get; set; }
    }
}
