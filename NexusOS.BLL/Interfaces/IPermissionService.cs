namespace NexusOS.BLL.Interfaces
{
    public interface IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissions(Guid userId, CancellationToken ct = default);
        public Task<bool> HasPermission(Guid userId, string permission, CancellationToken ct = default);
        public Task InvalidatePermissionCache();
    }
}
