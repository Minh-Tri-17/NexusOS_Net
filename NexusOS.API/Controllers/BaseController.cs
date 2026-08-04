using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusOS.BLL.Interfaces;
using NexusOS.MB;
using NexusOS.Util;

namespace NexusOS.API.Controllers
{
    // abstract: Ngăn chặn việc khởi tạo trực tiếp
    // virtual: Là các hàm có logic mặc định nhưng cho phép lớp con ghi đè
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Đặt ở đây để toàn bộ API đều cần xác thực
    public abstract class BaseController<TService, TEntity, TModel> : ControllerBase
        where TService : IBaseService<TEntity, TModel> // Đảm bảo Service truyền vào có đủ các hàm chuẩn (Create, Update, Delete) để gọi.
        where TEntity : class // Giới hạn kiểu dữ liệu phải là đối tượng để tương thích với EF Core và xử lý Null
        where TModel : class
    {
        protected readonly TService _service;

        protected BaseController(TService service)
        {
            _service = service;
        }

        [HttpPost(nameof(Create))]
        public virtual async Task<ActionResult> Create(TModel model)
        {
            var result = await _service.Create(model);
            return Ok(result);
        }

        [HttpPatch(nameof(Update))]
        public virtual async Task<ActionResult> Update(TModel model)
        {
            var result = await _service.Update(model);
            return Ok(result);
        }

        [HttpDelete(nameof(SoftDelete))]
        public virtual async Task<ActionResult> SoftDelete(string ids)
        {
            var result = await _service.SoftDelete(ids);
            return Ok(result);
        }

        [HttpDelete(nameof(HardDelete))]
        public virtual async Task<ActionResult> HardDelete(string ids)
        {
            var result = await _service.HardDelete(ids);
            return Ok(result);
        }

        [HttpGet($"{nameof(GetOne)}/{{id}}")]
        public virtual async Task<ActionResult> GetOne(Guid id)
        {
            var result = await _service.GetOne(id);
            return Ok(result);
        }

        [HttpPost($"{nameof(GetPaging)}/Filter")]
        public virtual async Task<ActionResult> GetPaging(FilterModel filter)
        {
            var result = await _service.GetPaging(filter);
            return Ok(result);
        }

        [HttpPost(nameof(Import))]
        public virtual async Task<ActionResult> Import(IFormFile file)
        {
            var result = await _service.Import(file);
            return Ok(result);
        }

        [HttpPost(nameof(Export))]
        public async Task<ActionResult<APIResults<byte[]>>> Export(FilterModel filter)
        {
            var result = await _service.Export(filter);

            if (!result.IsSuccess || result.Result == null)
                return Ok(result);

            return File(result.Result, AppConstants.ContentType, "export.xlsx"
            );
        }
    }
}
