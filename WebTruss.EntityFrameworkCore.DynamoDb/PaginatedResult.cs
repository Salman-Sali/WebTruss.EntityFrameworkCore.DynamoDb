namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class PaginatedResult<T>
    {
        public PaginatedResult(int currentPage)
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
}
