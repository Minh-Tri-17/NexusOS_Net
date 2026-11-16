using Microsoft.AspNetCore.Http;
using NexusOS.MB;

namespace NexusOS.BLL.Interfaces
{
    public interface IBaseService<TEntity, TModel>
        where TEntity : class // Giới hạn kiểu dữ liệu phải là đối tượng để tương thích với EF Core và xử lý Null
        where TModel : class
    {
        public Task<APIResults<PagingResults<TModel>>> GetPaging(FilterModel filter);
        public Task<APIResults<TModel>> GetOne(Guid id);
        public Task<APIResults<bool>> SoftDelete(List<Guid> ids);
        public Task<APIResults<bool>> HardDelete(List<Guid> ids);
        public Task<APIResults<bool>> Create(TModel request);
        public Task<APIResults<bool>> Update(TModel request);
        public Task<APIResults<byte[]>> Export(FilterModel filter);
        public Task<APIResults<bool>> Import(IFormFile fileImport);
    }
}
