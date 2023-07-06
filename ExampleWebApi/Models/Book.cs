using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace ExampleWebApi.Models
{
    public class Book
    {
        [Pk]
        [DynamoPropertyName("pk")]
        public string ISBN { get; set; } = null!;

        public string BookName { get; set; } = null!;

        public string AuthorId { get; set; } = null!;

        [DynamoLink(nameof(Book.AuthorId), typeof(Author), nameof(Author.Name))]
        public string AuthorName { get; set; } = null!;

        [DynamoLink("AuthorId", typeof(Author), "Description")]
        public string AuthorDescription { get; set; } = null!;
    }
}
