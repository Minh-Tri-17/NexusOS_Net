namespace NexusOS.BLL.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        Guid OwnerId { get; }
        string UserName { get; }
        string FullName { get; }
        string RoleId { get; }
    }
}
