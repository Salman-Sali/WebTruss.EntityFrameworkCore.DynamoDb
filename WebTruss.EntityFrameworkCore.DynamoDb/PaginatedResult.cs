namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class PaginatedResult<T>
    {
        public PaginatedResult()
        {
            Items = new List<T>();
        }
        public string? PaginationToken { get; set; }
        public List<T> Items { get; set; }
    }
}
