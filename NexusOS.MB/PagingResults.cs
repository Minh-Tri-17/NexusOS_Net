namespace NexusOS.MB
{
    public class PagingResults<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecord { get; set; }
        public int FromRecord => TotalRecord == 0 ? 0 : (PageIndex - 1) * PageSize + 1;
        public int ToRecord => Math.Min(PageIndex * PageSize, TotalRecord);
        public string RecordRange => $"{FromRecord} - {ToRecord}";
        public int PageCount
        {
            get
            {
                if (PageSize <= 0) return 0;
                return (int)Math.Ceiling((double)TotalRecord / PageSize);
            }
        }

        public PagingResults() { }

        public PagingResults(List<T> items, int totalRecord, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRecord = totalRecord;
            Items = items;
        }
    }
}
