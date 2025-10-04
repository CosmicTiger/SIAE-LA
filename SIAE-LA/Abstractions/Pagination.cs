namespace SIAE_LA.Abstractions
{
    public record QueryParams(int Page = 1, int PageSize = 20, string? Search = null)
    {
        public int Skip => (Page < 1 ? 0 : (Page - 1) * (PageSize < 1 ? 20 : PageSize));
        public int Take => PageSize is < 1 or > 200 ? 20 : PageSize;
    };
    public class PaginationResult<T>
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }
        public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    }
}
