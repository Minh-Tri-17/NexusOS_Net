using AutoMapper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using System.Reflection;

namespace NexusOS.Util
{
    public static class DataHelpers
    {
        private static IMapper? _mapper;

        public static void ConfigureMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        #region Handle data types

        public static string GetString(object? value)
        {
            return value?.ToString() ?? string.Empty;
        }

        public static Guid GetGuid(object? value)
        {
            if (value is Guid guid)
                return guid;

            // Nếu giá trị là string và có thể chuyển đổi thành Guid, thì thực hiện chuyển đổi
            if (value is string s && Guid.TryParse(s, out var result))
                return result;

            return Guid.Empty;
        }

        public static int GetInt(object? value)
        {
            if (value is int i)
                return i;

            // Nếu giá trị là string và có thể chuyển đổi thành int, thì thực hiện chuyển đổi
            if (value is string s && int.TryParse(s, out var result))
                return result;

            return 0;
        }

        public static double GetDouble(object? value)
        {
            if (value is double d)
                return d;

            // Nếu giá trị là string và có thể chuyển đổi thành double, thì thực hiện chuyển đổi
            if (value is string s && double.TryParse(s, out var result))
                return result;

            return 0.0;
        }

        public static decimal GetDecimal(object? value)
        {
            if (value is decimal d)
                return d;

            // Nếu giá trị là string và có thể chuyển đổi thành decimal, thì thực hiện chuyển đổi
            if (value is string s && decimal.TryParse(s, out var result))
                return result;

            return 0m;
        }

        public static DateTime GetDateTime(object? value)
        {
            if (value == null)
                return DateTime.MinValue;

            if (value is DateTime dt)
                return dt;

            // Nếu giá trị là string và có thể chuyển đổi thành DateTime, thì thực hiện chuyển đổi
            var stringDateTime = GetString(value);

            string[] formats = {
                AppConstants.DateFirstFormatDash,   // "dd-MM-yyyy"
                AppConstants.DateLastFormatDash,    // "yyyy-MM-dd"
                AppConstants.DateFirstFormatSlash,  // "dd/MM/yyyy"
                AppConstants.DateLastFormatSlash,   // "yyyy/MM/dd"
                AppConstants.DateTimeFormatDash,    // "dd-MM-yyyy HH:mm:ss"
                AppConstants.DateTimeFormatSlash    // "dd/MM/yyyy HH:mm:ss"
            };

            if (DateTime.TryParseExact(stringDateTime, formats, null, System.Globalization.DateTimeStyles.None, out var parsed))
                return parsed;

            // Thử Parse bình thường nếu không có định dạng khớp
            if (DateTime.TryParse(stringDateTime, out parsed))
                return parsed;

            return DateTime.MinValue;
        }

        public static bool GetBool(object? value)
        {
            if (value is bool b)
                return b;

            // Nếu giá trị là string và có thể chuyển đổi thành bool, thì thực hiện chuyển đổi
            if (value is string s && bool.TryParse(s, out var result))
                return result;

            return false;
        }

        #endregion

        #region Mapping data

        /// <summary>
        /// Dùng để mapping dữ liệu từ đối tượng này sang đối tượng khác
        /// Chỉ copy dữ liệu mà không tự sinh thêm các thuộc tính mặc định hệ thống (thường dùng cho get data)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Mapping<TSource, TDestination>(TSource source)
        {
            if (source == null || _mapper == null)
                return default!;

            return _mapper.Map<TDestination>(source, opt =>
            {
                // Không bỏ qua các trường audit, cho phép AutoMapper map bình thường từ source sang destination
                opt.Items["IgnoreAuditFields"] = false;
            });
        }

        /// <summary>
        /// Dùng để mapping dữ liệu từ danh sách đối tượng này sang danh sách đối tượng khác
        /// Chỉ copy dữ liệu mà không tự sinh thêm các thuộc tính mặc định hệ thống (thường dùng cho get data)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="sourceList"></param>
        /// <returns></returns>
        public static List<TDestination> MappingList<TSource, TDestination>(IEnumerable<TSource> sourceList)
        {
            if (sourceList == null)
                return new List<TDestination>();

            return sourceList.Select(Mapping<TSource, TDestination>).ToList();
        }

        /// <summary>
        /// Dùng để mapping dữ liệu từ đối tượng này sang đối tượng khác
        /// Copy dữ liệu và sinh tự động các thuộc tính mặc định hệ thống (thường dùng cho create và update data)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="userName"></param>
        public static void MapAudit<TSource, TDestination>(TSource source, TDestination destination,
            Guid? currentUser, DbSet<TDestination>? dbSet = null, IEnumerable<TDestination>? inMemoryList = null)
            where TDestination : class
        {
            if (source == null || destination == null || _mapper == null)
                return;

            var destinationProps = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            _mapper.Map<TSource, TDestination>(source, destination, opt =>
            {
                // Bỏ qua các trường audit khi map, tránh việc dữ liệu thô từ source đè mất logic tự sinh bên dưới
                opt.Items["IgnoreAuditFields"] = true;
            });

            #region Handle auto-generated fields

            // Lấy giá trị thuộc tính "ID" từ đối tượng đích (destination) để xác định xem là tạo mới hay cập nhật
            var idProp = destinationProps.FirstOrDefault(p => p.Name == AppConstants.Id);
            var idValue = idProp?.GetValue(destination);
            bool isNew = idValue == null || (idValue is Guid guid && guid == Guid.Empty);

            // Sinh mã code tự động nếu là bản ghi mới và thuộc tính Code tồn tại
            var tableName = GetString(GetTableName<TDestination>());
            if (tableName != null && tableName.StartsWith("Cat"))
            {
                tableName = tableName.Substring(3);
            }

            var codeProp = destinationProps.FirstOrDefault(p => p.Name == $"{tableName}{AppConstants.Code}" && p.PropertyType == typeof(string));
            if (codeProp != null)
            {
                var codeValue = codeProp.GetValue(destination) as string;
                if (string.IsNullOrEmpty(codeValue))
                {
                    var nameProp = destinationProps.FirstOrDefault(p =>
                        (p.Name == "Name" || p.Name == $"{tableName}Name") && p.PropertyType == typeof(string));
                    var nameValue = nameProp?.GetValue(destination) as string;

                    string prefix = GeneratePrefixFromName(nameValue, tableName);
                    string nextCode = $"{prefix}01";

                    if (dbSet != null)
                    {
                        // Lấy danh sách các mã đã có bắt đầu bằng prefix
                        var listCode = new List<string>();

                        var listExistingCode = dbSet
                            .Where(e => EF.Property<string>(e, codeProp.Name).StartsWith(prefix) && EF.Property<bool?>(e!, AppConstants.IsDelete) != true)
                            .Select(e => EF.Property<string>(e, codeProp.Name))
                            .ToList();

                        listCode.AddRange(listExistingCode);

                        if (inMemoryList != null)
                        {
                            var listLocalCode = inMemoryList
                                .Where(d => d != destination)
                                .Select(d => codeProp.GetValue(d) as string)
                                .OfType<string>()
                                .Where(c => c.StartsWith(prefix));

                            listCode.AddRange(listLocalCode);
                        }

                        int maxIndex = 0;

                        foreach (var code in listCode)
                        {
                            if (!string.IsNullOrEmpty(code) && code.Length > prefix.Length)
                            {
                                var numberPart = code.Substring(prefix.Length);

                                if (int.TryParse(numberPart, out int parsedNum) && parsedNum > maxIndex)
                                    maxIndex = parsedNum;
                            }
                        }

                        nextCode = $"{prefix}{(maxIndex + 1):D2}";

                        codeProp.SetValue(destination, nextCode);
                    }
                    else
                    {
                        codeProp.SetValue(destination, codeValue);
                    }
                }
                else
                {
                    codeProp.SetValue(destination, codeValue);
                }
            }

            // Sinh tự động thông tin khi tạo mới hoặc cập nhật
            if (isNew)
            {
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.Id)?.SetValue(destination, Guid.NewGuid());
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.CreatedAt)?.SetValue(destination, DateTime.Now);
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.CreatedBy)?.SetValue(destination, GetGuid(currentUser));

                // Tự động thiết lập thuộc tính IsDelete về false nếu nó tồn tại
                var isDeleteProp = destinationProps.FirstOrDefault(p => p.Name == AppConstants.IsDelete);
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.IsDelete)?.SetValue(destination, false);
            }
            else
            {
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.UpdatedAt)?.SetValue(destination, DateTime.Now);
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.UpdatedBy)?.SetValue(destination, GetGuid(currentUser));

                var valueDateCreate = destinationProps.FirstOrDefault(p => p.Name == AppConstants.CreatedAt)?.GetValue(destination);
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.CreatedAt)?.SetValue(destination, valueDateCreate);

                var valueIsDelete = destinationProps.FirstOrDefault(p => p.Name == AppConstants.IsDelete)?.GetValue(destination);
                destinationProps.FirstOrDefault(p => p.Name == AppConstants.IsDelete)?.SetValue(destination, valueIsDelete);
            }

            #endregion
        }

        /// <summary>
        /// Dùng để mapping dữ liệu từ list đối tượng này sang list đối tượng khác
        /// Copy dữ liệu và sinh tự động các thuộc tính mặc định hệ thống (thường dùng cho create và update data)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <param name="userName"></param>
        public static void MapListAudit<TSource, TDestination>(List<TSource> sourceList, List<TDestination> destinationList,
            Guid? userId, DbSet<TDestination>? dbSet = null) where TDestination : class, new()
        {
            if (sourceList == null || destinationList == null)
                return;

            var sourceIdProp = typeof(TSource).GetProperty(AppConstants.Id);
            var destinationIdProp = typeof(TDestination).GetProperty(AppConstants.Id);

            if (sourceIdProp == null || destinationIdProp == null)
                return;

            foreach (var source in sourceList)
            {
                var sourceId = sourceIdProp.GetValue(source);

                var destination = destinationList
                    .FirstOrDefault(d => destinationIdProp.GetValue(d)?.Equals(sourceId) == true);

                if (destination == null)
                {
                    destination = new TDestination();
                    MapAudit(source, destination, userId, dbSet, destinationList);
                    destinationList.Add(destination);
                }
                else
                {
                    MapAudit(source, destination, userId, dbSet, destinationList);
                }
            }
        }

        private static string GetTableName<T>()
        {
            return typeof(T).Name;
        }

        private static string GeneratePrefixFromName(string? rawName, string? fallbackTableName)
        {
            if (!string.IsNullOrWhiteSpace(rawName))
            {
                var words = rawName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Lấy chữ cái đầu của từng từ ghép lại: "Viet Nam" => "VN"
                var initials = words
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Select(w => char.ToUpperInvariant(w[0]))
                    .ToArray();

                if (initials.Length > 0)
                {
                    return new string(initials);
                }
            }

            // Fallback nếu Name rỗng: lấy 2 ký tự đầu của tableName
            if (!string.IsNullOrEmpty(fallbackTableName))
            {
                return fallbackTableName.Length >= 2
                    ? fallbackTableName.Substring(0, 2).ToUpperInvariant()
                    : fallbackTableName.ToUpperInvariant();
            }

            return "NS";
        }

        #endregion

        #region Import & Export

        /// <summary>
        /// Dùng để copy dữ liệu từ Excel sang đối tượng dựa vào tiêu đề cột
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T CopyImport<T>(IRow headerRow, IRow dataRow) where T : new()
        {
            var model = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var headerCell = headerRow.GetCell(i);
                var dataCell = dataRow.GetCell(i);

                // Bỏ qua nếu ô tiêu đề hoặc ô dữ liệu không tồn tại
                if (headerCell == null || dataCell == null)
                    continue;

                // Bỏ qua khi ô tiêu đề không có giá trị
                string? headerName = headerCell.ToString()?.Trim();
                if (string.IsNullOrEmpty(headerName))
                    continue;

                // Tìm thuộc tính trong class T có tên trùng với tiêu đề cột (không phân biệt chữ hoa/thường).
                var prop = props.FirstOrDefault(p =>
                    string.Equals(p.Name, headerName, StringComparison.OrdinalIgnoreCase));

                // Bỏ qua nếu không tìm thấy thuộc tính tương ứng hoặc không thể ghi
                if (prop == null || !prop.CanWrite)
                    continue;

                object? value = ConvertToPropertyType(prop.PropertyType, dataCell.ToString());
                prop.SetValue(model, value);
            }

            #region Set default values for system-defined attributes

            ResetSystemFields(model, props);

            #endregion

            return model;
        }

        /// <summary>
        /// Dùng để copy dữ liệu từ Excel sang đối tượng dựa vào key-value pair trong ô dữ liệu (áp dụng cho 1 model duy nhất)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T CopyImportTemplateSingle<T>(ISheet sheet) where T : new()
        {
            var model = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                var dataRow = sheet.GetRow(i);
                if (dataRow == null)
                    continue;

                foreach (var cell in dataRow.Cells)
                {
                    var cellValue = cell.ToString();
                    // Bỏ qua nếu ô dữ liệu không tồn tại hoặc không có giá trị
                    if (string.IsNullOrWhiteSpace(cellValue) || !cellValue.Contains("<:>"))
                        continue;

                    var parts = cellValue.Split(new[] { "<:>" }, StringSplitOptions.None);
                    if (parts.Length != 2)
                        continue;

                    var fieldName = parts[0].Split(' ').LastOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    var fieldValue = parts[^1].Trim();

                    // Tìm thuộc tính trong class T có tên trùng với tiêu đề cột (không phân biệt chữ hoa/thường).
                    var prop = props.FirstOrDefault(p =>
                        string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));

                    // Bỏ qua nếu không tìm thấy thuộc tính tương ứng hoặc không thể ghi
                    if (prop == null || !prop.CanWrite)
                        continue;

                    object? value = ConvertToPropertyType(prop.PropertyType, fieldValue);
                    prop.SetValue(model, value);
                }
            }

            #region Set default values for system-defined attributes

            ResetSystemFields(model, props);

            #endregion

            return model;
        }

        /// <summary>
        /// Dùng để copy dữ liệu từ Excel sang nhiều đối tượng dựa vào key-value pair trong ô dữ liệu
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        public static object[] CopyImportTemplateMulti(ISheet sheet, params object[] models)
        {
            // Lấy sẵn property list cho từng model để giảm chi phí reflection
            var listModelProps = models.Select(m => m.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)).ToArray();

            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                var dataRow = sheet.GetRow(i);
                if (dataRow == null)
                    continue;

                foreach (var cell in dataRow.Cells)
                {
                    var cellValue = cell.ToString();
                    // Bỏ qua nếu ô dữ liệu không tồn tại hoặc không có giá trị
                    if (string.IsNullOrWhiteSpace(cellValue) || !cellValue.Contains("<:>"))
                        continue;

                    var parts = cellValue.Split(new[] { "<:>" }, StringSplitOptions.None);
                    if (parts.Length != 2)
                        continue;

                    var fieldName = parts[0].Split(' ').LastOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    var fieldValue = parts[^1].Trim();

                    // Lặp qua tất cả model
                    for (int modelIndex = 0; modelIndex < models.Length; modelIndex++)
                    {
                        var props = listModelProps[modelIndex];
                        var model = models[modelIndex];

                        // Tìm thuộc tính trong class T có tên trùng với tiêu đề cột (không phân biệt chữ hoa/thường).
                        var prop = props.FirstOrDefault(p =>
                            string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));

                        // Bỏ qua nếu không tìm thấy thuộc tính tương ứng hoặc không thể ghi
                        if (prop == null || !prop.CanWrite)
                            continue;

                        object? valueConverted = ConvertToPropertyType(prop.PropertyType, fieldValue);
                        prop.SetValue(model, valueConverted);
                    }
                }
            }

            #region Set default values for system-defined attributes

            for (int modelIndex = 0; modelIndex < models.Length; modelIndex++)
            {
                var props = listModelProps[modelIndex];
                ResetSystemFields(models[modelIndex], props);
            }

            #endregion

            return models;
        }

        /// <summary>
        /// Gán giá trị mặc định cho các trường hệ thống trong đối tượng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        private static void ResetSystemFields<T>(T obj, PropertyInfo[] props)
        {
            var idProp = props.FirstOrDefault(p => p.Name == AppConstants.Id && p.PropertyType == typeof(Guid));
            var idValue = idProp?.GetValue(obj);

            if (idValue == null || GetGuid(idValue) == Guid.Empty)
                idProp?.SetValue(obj, Guid.Empty);
            else
                idProp?.SetValue(obj, idValue);

            string[] systemFields = {
                AppConstants.CreatedAt,
                AppConstants.UpdatedAt,
                AppConstants.CreatedBy,
                AppConstants.UpdatedBy,
                AppConstants.IsDelete,
                typeof(T).Name + AppConstants.Code
            };

            foreach (var field in systemFields)
            {
                var prop = props.FirstOrDefault(p => p.Name == field);
                prop?.SetValue(obj, null);
            }
        }

        /// <summary>
        /// Chuyển đổi chuỗi thành kiểu dữ liệu của thuộc tính
        /// </summary>
        /// <param name="propType"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private static object? ConvertToPropertyType(Type propType, string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                if (Nullable.GetUnderlyingType(propType) != null)
                    return null;

                if (propType == typeof(Guid)) return Guid.Empty;
                if (propType == typeof(string)) return null;
                if (propType == typeof(int)) return 0;
                if (propType == typeof(decimal)) return 0m;
                if (propType == typeof(double)) return 0d;
                if (propType == typeof(DateTime)) return DateTime.MinValue;
                if (propType == typeof(bool)) return false;

                return null;
            }

            Type actualType = Nullable.GetUnderlyingType(propType) ?? propType;

            if (actualType == typeof(string))
                return str;

            if (actualType == typeof(Guid))
                return GetGuid(str);

            if (actualType == typeof(decimal))
                return GetDecimal(str);

            if (actualType == typeof(double))
                return GetDouble(str);

            if (actualType == typeof(int))
                return GetInt(str);

            if (actualType == typeof(DateTime))
                return GetDateTime(str);

            if (actualType == typeof(bool))
                return GetBool(str);

            return str;
        }

        /// <summary>
        /// Dùng để copy dữ liệu từ đối tượng sang Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worksheet"></param>
        /// <param name="items"></param>
        /// <param name="headers"></param>
        public static void CopyExport<T>(IXLWorksheet worksheet, List<T> items)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var headers = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(s => s.GetMethod != null
                    && (!s.GetMethod.IsVirtual || s.GetMethod.IsFinal))
                .Select(p => p.Name)
                .ToList();

            // Header row
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Italic = true;
                cell.Style.Font.FontSize = 14;
                cell.Style.Font.FontColor = XLColor.FromHtml("#776b5d");
            }

            // Data rows
            int row = 2;
            foreach (var item in items)
            {
                for (int col = 0; col < headers.Count; col++)
                {
                    string header = headers[col];
                    var prop = props.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                    if (prop == null)
                        continue;

                    object? value = prop.GetValue(item);
                    worksheet.Cell(row, col + 1).SetValue(FormatValue(value)?.ToString() ?? string.Empty);
                }
                row++;
            }
        }

        /// <summary>
        /// Dùng để định dạng giá trị trước khi xuất ra Excel
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object? FormatValue(object? value)
        {
            switch (value)
            {
                case null:
                    return null;
                case DateTime dt:
                    return dt.ToString(AppConstants.DateTimeFormatDash);
                case bool b:
                    return b ? "Yes" : "No";
                case Enum e:
                    return e.ToString();
                default:
                    return value.ToString();
            }
        }

        #endregion
    }
}
