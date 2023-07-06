using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace ExampleWebApi.Models
{
    public class Author
    {
        [Pk]
        [DynamoPropertyName("pk")]
        public string AuthorId { get; set; } = null!;

        public string Name { get; set; } = null!;

        [DynamoPropertyName("Description")]
        public string? Desc { get; set; } = null!;
    }
}
