namespace NexusOS.Util
{
    public enum FilterOperator
    {
        /// <summary>
        /// So khớp tương đối, thường dùng cho string (SQL: LIKE '%value%')
        /// </summary>
        Like,

        /// <summary>
        /// Bằng nhau (=)
        /// </summary>
        Equal,

        /// <summary>
        /// Khác nhau (!=)
        /// </summary>
        NotEqual,

        /// <summary>
        /// Lớn hơn (>)
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Lớn hơn hoặc bằng (>=)
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// Nhỏ hơn (<)
        /// </summary>
        LessThan,

        /// <summary>
        /// Nhỏ hơn hoặc bằng (<=)
        /// </summary>
        LessOrEqual,

        /// <summary>
        /// Trong khoảng từ ... đến ... (áp dụng cho số hoặc ngày)
        /// </summary>
        Between,

        /// <summary>
        /// Chuỗi chứa giá trị con (string.Contains) → SQL: LIKE '%value%'
        /// </summary>
        Contains
    }

    public enum FilterType
    {
        String,
        Number,
        Date,
        Boolean,
        Guid,
    }
}
