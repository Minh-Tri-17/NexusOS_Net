namespace NexusOS.BLL.Interfaces
{
    public interface IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissions(Guid userId);
        public Task<bool> HasPermission(Guid userId, string permission);
        public Task InvalidatePermissionCache();
    }
}
