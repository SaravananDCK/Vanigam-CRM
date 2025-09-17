namespace Vanigam.CRM.Objects.Contracts
{
    public interface IHasSoftDelete
    {
        bool IsNotDeleted { get; set; }
    }
}

