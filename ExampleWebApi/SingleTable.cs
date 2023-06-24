using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace ExampleWebApi
{
    public class SingleTable
    {
        [Pk]
        [DynamoPropertyName("pk")]
        public string Pk { get; set; }

        [Sk]
        [DynamoPropertyName("sk")]
        public string Sk { get; set; }

    }
}
