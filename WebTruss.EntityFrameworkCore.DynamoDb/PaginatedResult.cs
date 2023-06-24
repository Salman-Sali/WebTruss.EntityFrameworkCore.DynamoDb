namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class QueriedPaginatedResult<T>
    {
        public QueriedPaginatedResult(int currentPage)
        {
            Items = new List<T>();
            PreviousToken = null;
            NextToken = null;
            CurrentPage = currentPage;
        }

        public int CurrentPage { get; set; }
        public string? PreviousToken { get; set; }
        public string? NextToken { get; set; }
        public List<T> Items { get; set; }
    }

    public class ScannedPaginatedResult<T>
    {
        public ScannedPaginatedResult()
        {
            Items = new List<T>();
        }
        public string? PaginationToken { get; set; }
        public List<T> Items { get; set; }
    }
}
