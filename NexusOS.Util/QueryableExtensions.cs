using Microsoft.EntityFrameworkCore;
using NexusOS.MB;

namespace NexusOS.Util
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Áp dụng bộ lọc động dựa trên tham số truyền lên từ Client (FilterModel)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IQueryable<T> ApplyCommonFilters<T>(this IQueryable<T> query, FilterModel filter)
        {
            foreach (var item in filter.Filters)
            {
                var propName = item.FilterName;
                var propType = item.FilterType;
                var propOperator = item.FilterOperator;
                var filterValue = item.FilterValue;

                if (string.IsNullOrEmpty(propName))
                    continue;

                switch (propType, propOperator)
                {
                    case (nameof(FilterType.String), nameof(FilterOperator.Like)):
                        {
                            var propValue = DataHelpers.GetString(filterValue);
                            if (!string.IsNullOrWhiteSpace(propValue))
                                query = query.Where(s => EF.Functions.Like(EF.Property<string>(s!, propName).ToLower(),
                                    $"%{propValue.Trim().ToLower()}%"));

                            break;
                        }
                    case (nameof(FilterType.String), nameof(FilterOperator.Contains)):
                        {
                            var propValue = DataHelpers.GetString(filterValue);
                            if (!string.IsNullOrWhiteSpace(propValue))
                                query = query.Where(s => EF.Property<string>(s!, propName).Contains(propValue));

                            break;
                        }
                    case (nameof(FilterType.Guid), nameof(FilterOperator.Contains)):
                        {
                            var listGuids = DataHelpers.GetString(filterValue)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => DataHelpers.GetGuid(id.Trim()))
                                .Where(guid => guid != Guid.Empty)
                                .ToList();

                            if (listGuids.Any())
                                query = query.Where(s => listGuids.Contains(EF.Property<Guid>(s!, AppConstants.Id)));

                            break;
                        }
                    case (nameof(FilterType.Date), nameof(FilterOperator.Equal)):
                        {
                            var propValue = DataHelpers.GetDateTime(filterValue);
                            if (propValue != DateTime.MinValue)
                                query = query.Where(s => EF.Functions.DateDiffDay(EF.Property<DateTime?>(s!, propName)!.Value,
                                    propValue) == 0);

                            break;
                        }
                    case (nameof(FilterType.Number), nameof(FilterOperator.Equal)):
                        {
                            var propValue = DataHelpers.GetInt(filterValue);
                            if (propValue != 0)
                                query = query.Where(s => EF.Property<int?>(s!, propName) == propValue);

                            break;
                        }
                    case (nameof(FilterType.Boolean), nameof(FilterOperator.Equal)):
                        {
                            query = query.Where(s => EF.Property<bool?>(s!, propName) == true);

                            break;
                        }
                    case (nameof(FilterType.Boolean), nameof(FilterOperator.NotEqual)):
                        {
                            query = query.Where(s => EF.Property<bool?>(s!, propName) != true);

                            break;
                        }
                }
            }

            return query;
        }

        /// <summary>
        /// Giới hạn số lượng bản ghi trả về từ Database dựa trên vị trí trang và kích thước trang yêu cầu.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, FilterModel filter)
        {
            if (filter.AllowPaging)
            {
                int skipCount = Math.Max(0, (filter.PageIndex - 1) * filter.PageSize);

                query = query.Skip(skipCount).Take(filter.PageSize);
            }

            return query;
        }

        /// <summary>
        /// Tự động kiểm tra và thêm điều kiện lọc các bản ghi "đã xóa tạm"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> ApplySoftDelete<T>(this IQueryable<T> query, FilterModel filter)
        {
            var prop = typeof(T).GetProperty(AppConstants.IsDelete);

            if (prop != null)
            {
                bool isDeleted = DataHelpers.GetBool(filter.Filters.FirstOrDefault(s =>
                    s.FilterName == AppConstants.IsDelete)?.FilterValue);

                if (isDeleted)
                {
                    return query.Where(s => EF.Property<bool?>(s!, AppConstants.IsDelete) == true);
                }
                else
                {
                    return query.Where(s => EF.Property<bool?>(s!, AppConstants.IsDelete) != true);
                }
            }

            return query;
        }

        /// <summary>
        /// Tự động kiểm tra và sắp xếp lại các bản ghi 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query)
        {
            var propUAt = typeof(T).GetProperty(AppConstants.UpdatedAt);
            var propCAt = typeof(T).GetProperty(AppConstants.CreatedAt);

            if (propUAt != null && propCAt != null)
            {
                return query.OrderByDescending(s =>
                    EF.Property<DateTime?>(s!, AppConstants.UpdatedAt) ?? EF.Property<DateTime>(s!, AppConstants.CreatedAt));
            }

            if (propUAt != null)
            {
                return query.OrderByDescending(s => EF.Property<DateTime?>(s!, AppConstants.UpdatedAt));
            }

            if (propCAt != null)
            {
                return query.OrderByDescending(s => EF.Property<DateTime?>(s!, AppConstants.CreatedAt));
            }

            return query;
        }
    }
}
