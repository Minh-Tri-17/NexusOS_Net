using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;
using NPOI.XSSF.UserModel;

namespace NexusOS.BLL.Services
{
    // abstract: Ngăn chặn việc khởi tạo trực tiếp
    // virtual: Là các hàm có logic mặc định nhưng cho phép lớp con ghi đè
    public abstract class BaseService<TEntity, TModel>
        where TEntity : class, IBaseEntity, new()
        where TModel : class, new()
    {
        protected readonly NexusOsContext _context; // Dùng để truy cập vào DbContext
        protected readonly ICurrentUserService _currentUser; // Dùng để lấy thông tin người dùng hiện tại
        protected readonly DbSet<TEntity> _dbSet; // Dùng để thao tác với tập thực thể
        protected readonly IStringLocalizer _localizer; // Dùng để đa ngôn ngữ hóa thông báo

        protected BaseService(NexusOsContext context, ICurrentUserService currentUser, IStringLocalizer localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<APIResults<bool>> Create(TModel request)
        {
            if (request == null)
                return APIResults<bool>.Failure(_localizer[Messages.UpdateFailure]);

            // Bắt đầu một giao dịch mới để nhóm các thao tác cơ sở dữ liệu lại với nhau.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                TEntity entity = new TEntity();

                await BeforeSaveAsync(request, entity, true);

                // Map dữ liệu từ request sang entity và gắn Audit (UserId, CreatedAt...)
                DataHelpers.MapAudit(request, entity, _currentUser.UserId, _context);

                await _dbSet.AddAsync(entity);

                await AfterSaveAsync(request, entity);

                var result = await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Lưu vĩnh viễn mọi thay đổi trong giao dịch vào cơ sở dữ liệu một cách an toàn.

                if (result > 0)
                {
                    return APIResults<bool>.Success(true, _localizer[Messages.CreateSuccess]);
                }
                else
                {
                    return APIResults<bool>.Failure(_localizer[Messages.CreateFailure]);
                }
            }
            catch
            {
                await transaction.RollbackAsync(); // Hủy bỏ toàn bộ các thay đổi trong giao dịch khi xảy ra lỗi.
                throw;
            }
        }

        public virtual async Task<APIResults<bool>> Update(TModel request)
        {
            if (request == null)
                return APIResults<bool>.Failure(_localizer[Messages.UpdateFailure]);

            // 1. Lấy Id từ request bằng dynamic để tránh lỗi biên dịch do TModel chưa xác định có Id hay không
            var requestId = (request as dynamic)?.Id?.ToString();
            var id = DataHelpers.GetGuid(requestId);

            // Bắt đầu một giao dịch mới để nhóm các thao tác cơ sở dữ liệu lại với nhau.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                TEntity entity = await _dbSet.FindAsync(id);
                if (entity == null)
                    return APIResults<bool>.Failure(_localizer[Messages.NotFoundUpdate]);

                await BeforeSaveAsync(request, entity, false);

                // Map đè dữ liệu mới từ request vào entity đang theo dõi (Tracking)
                DataHelpers.MapAudit(request, entity, _currentUser.UserId, _context);

                await AfterSaveAsync(request, entity);

                var result = await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Lưu vĩnh viễn mọi thay đổi trong giao dịch vào cơ sở dữ liệu một cách an toàn.

                if (result > 0)
                {
                    return APIResults<bool>.Success(true, _localizer[Messages.UpdateSuccess]);
                }
                else
                {
                    return APIResults<bool>.Failure(_localizer[Messages.UpdateFailure]);
                }
            }
            catch
            {
                await transaction.RollbackAsync(); // Hủy bỏ toàn bộ các thay đổi trong giao dịch khi xảy ra lỗi.
                throw;
            }
        }

        protected virtual Task BeforeSaveAsync(TModel request, TEntity entity, bool isNew) => Task.CompletedTask;

        protected virtual Task AfterSaveAsync(TModel request, TEntity entity) => Task.CompletedTask;

        public virtual async Task<APIResults<bool>> SoftDelete(string ids)
        {
            var listIds = ids.Split(',').Select(id => DataHelpers.GetGuid(id)).ToList();
            var result = await _dbSet
                .Where(s => listIds.Contains(s.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDelete, true));

            return result > 0
                ? APIResults<bool>.Success(true, _localizer[Messages.DeleteSuccess])
                : APIResults<bool>.Failure(_localizer[Messages.DeleteFailure]);
        }

        public virtual async Task<APIResults<bool>> HardDelete(string ids)
        {
            var listIds = ids.Split(',').Select(id => DataHelpers.GetGuid(id)).ToList();
            var result = await _dbSet
                .Where(s => listIds.Contains(s.Id))
                .ExecuteDeleteAsync();

            return result > 0
                ? APIResults<bool>.Success(true, _localizer[Messages.DeleteSuccess])
                : APIResults<bool>.Failure(_localizer[Messages.DeleteFailure]);
        }

        public virtual async Task<APIResults<TModel>> GetOne(Guid id)
        {
            var entity = await _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null) return APIResults<TModel>.Failure(_localizer[Messages.NotFoundGet]);

            var model = DataHelpers.Mapping<TEntity, TModel>(entity);
            return APIResults<TModel>.Success(model, _localizer[Messages.GetResultSuccess]);
        }

        public virtual async Task<APIResults<PagingResults<TModel>>> GetPaging(FilterModel filter)
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .ApplySort()
                .ApplySoftDelete(filter)
                .ApplyCommonFilters(filter);

            var totalCount = await query.CountAsync();
            query = query.ApplyPaging(filter);

            var list = await query.ToListAsync();
            var listModel = DataHelpers.MappingList<TEntity, TModel>(list);

            var pageResult = new PagingResults<TModel>
            {
                TotalRecord = totalCount,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                Items = listModel
            };

            return APIResults<PagingResults<TModel>>.Success(pageResult, _localizer[Messages.GetListResultSuccess]);
        }

        public virtual async Task<APIResults<byte[]>> Export(FilterModel filter)
        {
            var dataResult = await GetPaging(filter);
            var items = dataResult?.Result?.Items ?? new List<TModel>();

            using var workbook = new XLWorkbook();
            // Lấy tên Class Entity làm tên Sheet
            var sheetName = typeof(TEntity).Name;
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Chuyển Model về Entity để DataHelpers xử lý export
            var listEntity = DataHelpers.MappingList<TModel, TEntity>(items);
            DataHelpers.CopyExport(worksheet, listEntity);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();

            return bytes.Length > 0
                ? APIResults<byte[]>.Success(bytes, _localizer[Messages.ExportSuccess])
                : APIResults<byte[]>.Failure(_localizer[Messages.ExportFailure]);
        }

        public virtual async Task<APIResults<bool>> Import(IFormFile fileImport)
        {
            if (fileImport == null || fileImport.Length <= 0)
                return APIResults<bool>.Failure(AppConstants.FileNotFound);

            using var stream = new MemoryStream();
            await fileImport.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            var headerRow = sheet.GetRow(0);

            var listModel = new List<TModel>();

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                // Bỏ qua nếu hàng trống
                if (row == null || row.Cells.All(c => c.CellType == NPOI.SS.UserModel.CellType.Blank))
                    continue;

                TModel model = DataHelpers.CopyImport<TModel>(headerRow, row);
                listModel.Add(model);
            }

            var listEntity = new List<TEntity>();
            // Map từ Model sang Entity và gắn UserId để Audit
            DataHelpers.MapListAudit<TModel, TEntity>(listModel, listEntity, _currentUser.UserId, _context);

            try
            {
                // Tắt theo dõi thay đổi để tăng tốc độ nạp dữ liệu
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                await _dbSet.AddRangeAsync(listEntity);
                var result = await _context.SaveChangesAsync();

                return result > 0
                    ? APIResults<bool>.Success(true, _localizer[Messages.ImportSuccess])
                    : APIResults<bool>.Failure(_localizer[Messages.ImportFailure]);
            }
            finally
            {  // Bật lại hoặc Clear tracker
                _context.ChangeTracker.Clear();
            }
        }
    }
}
