namespace Blogsphere.Webapp.Bff.Domain.Models.Core
{
    public class PaginatedResult<T>(int pageIndex, int pageSize, long count, IReadOnlyList<T> data)
        where T : class
    {
        public int PageIndex { get; set; } = pageIndex;
        public int PageSize { get; set; } = pageSize;
        public long Count { get; set; } = count;
        public IReadOnlyList<T> Data { get; set; } = data;
    }

    public class PaginatedResultV2<T> where T : class
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public long TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public IReadOnlyList<T> Items { get; set; }
    }
}