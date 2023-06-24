namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class QueriedPaginatedResult<T>
    {
        public QueriedPaginatedResult()
        {
            Items = new List<T>();
            PreviousToken = null;
            NextToken = null;
        }
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
