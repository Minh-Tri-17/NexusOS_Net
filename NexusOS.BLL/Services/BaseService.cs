using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NexusOS.BLL.Interfaces;
using NexusOS.DAL;
using NexusOS.DAL.Models;
using NexusOS.MB;
using NexusOS.Util;

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
        protected readonly IStringLocalizer<SharedResource> _localizer; // Dùng để đa ngôn ngữ hóa thông báo

        protected BaseService(NexusOsContext context, ICurrentUserService currentUser, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<APIResults<bool>> Create(TModel request)
        {
            if (request == null)
                return APIResults<bool>.Failure(_localizer[Messages.CreateFailure]);

            try
            {
                // Bắt đầu một giao dịch mới để nhóm các thao tác cơ sở dữ liệu lại với nhau.
                await using var transaction = await _context.Database.BeginTransactionAsync();

                TEntity entity = new TEntity();

                await BeforeSaveAsync(request, entity, true);

                // Map dữ liệu từ request sang entity và gắn Audit (UserId, CreatedAt...)
                DataHelpers.MapAudit(request, entity, _currentUser.UserId, _dbSet);

                await _dbSet.AddAsync(entity);

                await AfterSaveAsync(request, entity);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Lưu vĩnh viễn mọi thay đổi trong giao dịch vào cơ sở dữ liệu một cách an toàn.

                return APIResults<bool>.Success(true, _localizer[Messages.CreateSuccess]);
            }
            catch
            {
                return APIResults<bool>.Failure(_localizer[Messages.CreateFailure]);
            }
        }

        public virtual async Task<APIResults<bool>> Update(TModel request)
        {
            if (request == null)
                return APIResults<bool>.Failure(_localizer[Messages.UpdateFailure]);

            try
            {
                // Lấy Id từ request bằng dynamic để tránh lỗi biên dịch do TModel chưa xác định có Id hay không
                var requestId = (request as dynamic)?.Id?.ToString();
                var id = DataHelpers.GetGuid(requestId);

                // Bắt đầu một giao dịch mới để nhóm các thao tác cơ sở dữ liệu lại với nhau.
                await using var transaction = await _context.Database.BeginTransactionAsync();

                TEntity entity = await _dbSet.FindAsync(id);
                if (entity == null)
                    return APIResults<bool>.Failure(_localizer[Messages.NotFoundUpdate]);

                await BeforeSaveAsync(request, entity, false);

                // Map đè dữ liệu mới từ request vào entity đang theo dõi (Tracking)
                DataHelpers.MapAudit(request, entity, _currentUser.UserId, _dbSet);

                await AfterSaveAsync(request, entity);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Lưu vĩnh viễn mọi thay đổi trong giao dịch vào cơ sở dữ liệu một cách an toàn.

                return APIResults<bool>.Success(true, _localizer[Messages.UpdateSuccess]);
            }
            catch
            {
                return APIResults<bool>.Failure(_localizer[Messages.UpdateFailure]);
            }
        }

        protected virtual Task BeforeSaveAsync(TModel request, TEntity entity, bool isNew) => Task.CompletedTask;

        protected virtual Task AfterSaveAsync(TModel request, TEntity entity) => Task.CompletedTask;

        public virtual async Task<APIResults<bool>> SoftDelete(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return APIResults<bool>.Failure(_localizer[Messages.DeleteFailure]);

            int result = 0;

            foreach (var selectIds in ids.Chunk(1000))
            {
                result += await _dbSet
                   .Where(s => selectIds.Contains(s.Id))
                   .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDelete, true));
            }

            return result > 0
                ? APIResults<bool>.Success(true, _localizer[Messages.DeleteSuccess])
                : APIResults<bool>.Failure(_localizer[Messages.DeleteFailure]);
        }

        public virtual async Task<APIResults<bool>> HardDelete(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return APIResults<bool>.Failure(_localizer[Messages.DeleteFailure]);

            int result = 0;

            foreach (var selectIds in ids.Chunk(1000))
            {
                result += await _dbSet
                    .Where(s => selectIds.Contains(s.Id))
                    .ExecuteDeleteAsync();
            }

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
            _context.Database.SetCommandTimeout(120); // Tăng thời gian timeout lên 120 giây(mặc định là 30s)

            IQueryable<TEntity> query = _dbSet.AsNoTracking() // Tắt cơ chế "theo dõi thay đổi" (Change Tracking) của Entity Framework
                .ApplySort()
                .ApplySoftDelete(filter)
                .ApplyCommonFilters(filter);

            var totalCount = await query.CountAsync();
            query = query.ApplyPaging(filter);

            var entities = await query.ToListAsync();
            var models = DataHelpers.MappingList<TEntity, TModel>(entities);

            var pageResult = new PagingResults<TModel>(models, totalCount, filter.PageIndex, filter.PageSize);

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
            var entities = DataHelpers.MappingList<TModel, TEntity>(items);
            DataHelpers.CopyExport(worksheet, entities);

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

            try
            {
                var models = new List<TModel>();

                using var stream = fileImport.OpenReadStream();
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Đọc dòng Header đầu tiên
                    if (!reader.Read())
                        return APIResults<bool>.Failure(_localizer[Messages.ImportFailure]);

                    var headers = new List<string>();

                    for (int col = 0; col < reader.FieldCount; col++)
                    {
                        headers.Add(reader.GetValue(col)?.ToString()?.Trim() ?? string.Empty);
                    }

                    while (reader.Read())
                    {
                        bool isEmptyRow = true;

                        for (int col = 0; col < reader.FieldCount; col++)
                        {
                            var val = reader.GetValue(col);
                            if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                            {
                                isEmptyRow = false;
                                break;
                            }
                        }

                        if (isEmptyRow) continue;

                        TModel model = DataHelpers.CopyImport<TModel>(headers, reader);
                        models.Add(model);
                    }
                }

                var idProp = typeof(TModel).GetProperty(AppConstants.Id);
                if (idProp == null)
                    return APIResults<bool>.Failure(_localizer[Messages.ImportFailure]);

                var modelIds = models
                    .Select(x => DataHelpers.GetGuid(idProp.GetValue(x)))
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                var existingEntities = new List<TEntity>();

                foreach (var selectModelIds in modelIds.Chunk(1000))
                {
                    FilterModel filter = new FilterModel()
                    {
                        Filters = new List<FilterItemModel>()
                        {
                            new FilterItemModel
                            {
                                FilterName = AppConstants.Id,
                                FilterType = FilterType.Guid.ToString(),
                                FilterOperator = FilterOperator.Contains.ToString(),
                                FilterValue = string.Join(',', selectModelIds)
                            }
                        }
                    };

                    var results = await _dbSet.AsNoTracking()
                        .ApplySoftDelete(filter)
                        .ApplyCommonFilters(filter)
                        .ToListAsync();

                    existingEntities.AddRange(results);
                }

                // Chỉ những Id thực sự có trong DB mới là Update
                var existingIds = existingEntities
                    .Select(e => e.Id)
                    .ToHashSet();

                var entities = existingEntities.ToList();

                // Map từ Model sang Entity và gắn UserId để Audit
                DataHelpers.MapListAudit<TModel, TEntity>(models, entities, _currentUser.UserId, _dbSet);

                var insertEntities = entities
                    .Where(e => !existingIds.Contains(e.Id))
                    .ToList();

                var updateEntities = entities
                    .Where(e => existingIds.Contains(e.Id))
                    .ToList();

                var originalAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;

                try
                {
                    // Bắt đầu một giao dịch mới để nhóm các thao tác cơ sở dữ liệu lại với nhau.
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    // Tắt theo dõi thay đổi để tăng tốc độ nạp dữ liệu
                    _context.ChangeTracker.AutoDetectChangesEnabled = false;

                    const int batchSize = 1000;

                    // Insert theo từng batch để tránh lỗi OutOfMemory
                    foreach (var batch in insertEntities.Chunk(batchSize))
                    {
                        _dbSet.AddRange(batch);

                        await _context.SaveChangesAsync();

                        // Xóa cache tracker sau khi đã lưu thành công để giải phóng RAM
                        _context.ChangeTracker.Clear();
                    }

                    // Update theo từng batch để tránh lỗi OutOfMemory
                    foreach (var batch in updateEntities.Chunk(batchSize))
                    {
                        _dbSet.UpdateRange(batch);

                        await _context.SaveChangesAsync();

                        // Xóa cache tracker sau khi đã lưu thành công để giải phóng RAM
                        _context.ChangeTracker.Clear();
                    }

                    await transaction.CommitAsync();

                    return APIResults<bool>.Success(true, _localizer[Messages.ImportSuccess]);
                }
                finally
                {
                    // Khôi phục lại trạng thái mặc định của ChangeTracker cho các tác vụ khác
                    _context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
                }
            }
            catch (Exception)
            {
                return APIResults<bool>.Failure(_localizer[Messages.ImportFailure]);
            }
        }
    }
}
