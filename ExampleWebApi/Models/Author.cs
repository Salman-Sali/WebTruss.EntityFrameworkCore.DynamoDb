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

        public List<EducationEntry>? Education { get; set; }
        public Address? Address { get; set; }
    }

    public class EducationEntry
    {
        public string University { get; set; } = null!;
        public string Course { get; set; } = null!;
    }

    public class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}
