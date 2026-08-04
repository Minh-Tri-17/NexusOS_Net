namespace NexusOS.Util
{
    public class AppConstants
    {
        #region Base

        public const string Id = "Id";
        public const string Code = "Code";
        public const string CreatedAt = "CreatedAt";
        public const string CreatedBy = "CreatedBy";
        public const string UpdatedAt = "UpdatedAt";
        public const string UpdatedBy = "UpdatedBy";
        public const string IsDelete = "IsDelete";
        public const string PageIndex = "PageIndex";
        public const string PageSize = "PageSize";
        public const string FileNotFound = "FileNotFound";
        public const string Context = "Context";

        #endregion

        #region Format

        // File & Export
        public const string FileName = "{0}_{1}.xlsx";
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        // Currency & Numbers
        public const string Currency = "$";
        public const string CurrencyFormat = "$#,##0.00";
        public const string NumberFormat = "#,##0";
        public const string DecimalFormat = "#,##0.00";
        public const string PercentageFormat = "0.00%";

        // Date & Time Formats
        public const string DateTimeString = "yyyyMMddHHmmssfff";
        public const string FileNameDateTimeFormat = "yyyyMMdd_HHmmss";
        public const string IsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public const string DateFirstFormatDash = "dd-MM-yyyy";
        public const string DateLastFormatDash = "yyyy-MM-dd";
        public const string DateFirstFormatSlash = "dd/MM/yyyy";
        public const string DateLastFormatSlash = "yyyy/MM/dd";

        public const string DateTimeFormatDash = "dd-MM-yyyy HH:mm:ss";
        public const string DateTimeFormatSlash = "dd/MM/yyyy HH:mm:ss";

        public const string TimeFormat = "HH:mm:ss";
        public const string MonthYearFormatSlash = "MM/yyyy";

        #endregion
    }
}
