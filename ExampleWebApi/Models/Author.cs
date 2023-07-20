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
        public string? University { get; set; } = null!;
        public string Course { get; set; } = null!;
    }

    public class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public CountryInformation? Country { get; set; }
        public List<AddressPostOffices>? PostOffices { get; set; }//just random stuff to test out
    }

    public class CountryInformation
    {
        public string Country { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class AddressPostOffices
    {
        public string PostOfficeName { get; set; } = null!;
    }
}
