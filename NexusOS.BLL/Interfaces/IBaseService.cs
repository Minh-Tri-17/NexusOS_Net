using Microsoft.AspNetCore.Http;
using NexusOS.MB;

namespace NexusOS.BLL.Interfaces
{
    public interface IBaseService<TEntity, TModel>
        where TEntity : class // Giới hạn kiểu dữ liệu phải là đối tượng để tương thích với EF Core và xử lý Null
        where TModel : class
    {
        Task<APIResults<PagingResults<TModel>>> GetPaging(FilterModel filter);
        Task<APIResults<TModel>> GetOne(Guid id);
        Task<APIResults<bool>> SoftDelete(string ids);
        Task<APIResults<bool>> HardDelete(string ids);
        Task<APIResults<bool>> Create(TModel request);
        Task<APIResults<bool>> Update(TModel request);
        Task<APIResults<byte[]>> Export(FilterModel filter);
        Task<APIResults<bool>> Import(IFormFile fileImport);
    }
}
