namespace NexusOS.DAL
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
        bool? IsDelete { get; set; }
        DateTime? CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
